<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Item\StoreItemRequest;
use App\Http\Requests\Api\Item\UpdateItemRequest;
use App\Http\Resources\ItemResource;
use App\Models\Item;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use Illuminate\Support\Facades\Gate;

class ItemController extends Controller
{
    /**
     * Display a listing of inventory items with optional search filter by name/flavor.
     */
    public function index(Request $request): AnonymousResourceCollection
    {
        Gate::authorize('viewAny', Item::class);

        $searchTerm = $request->query('name') ?? $request->query('search');

        $items = Item::with('category')
            ->when($searchTerm, fn ($query) => $query->search($searchTerm))
            ->when($request->query('category_id'), fn ($query, $catId) => $query->where('category_id', $catId))
            ->latest('id')
            ->paginate($request->query('per_page', 15));

        return ItemResource::collection($items);
    }

    /**
     * Store a newly created inventory item.
     */
    public function store(StoreItemRequest $request): JsonResponse
    {
        $item = Item::create($request->validated());

        return (new ItemResource($item->load('category')))
            ->response()
            ->setStatusCode(201);
    }

    /**
     * Display the specified inventory item.
     */
    public function show(Item $item): ItemResource
    {
        Gate::authorize('view', $item);

        return new ItemResource($item->load('category'));
    }

    /**
     * Update the specified inventory item.
     */
    public function update(UpdateItemRequest $request, Item $item): ItemResource
    {
        $item->update($request->validated());

        return new ItemResource($item->load('category'));
    }

    /**
     * Remove the specified inventory item.
     * Allowed for Owner only.
     */
    public function destroy(Item $item): JsonResponse
    {
        Gate::authorize('delete', $item);

        $item->delete();

        return response()->json([
            'message' => 'Inventory item deleted successfully.',
        ], 200);
    }
}
