<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Http\Requests\Api\Customer\StoreCustomerRequest;
use App\Http\Requests\Api\Customer\UpdateCustomerRequest;
use App\Http\Resources\CustomerResource;
use App\Models\Customer;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Http\Resources\Json\AnonymousResourceCollection;
use Illuminate\Support\Facades\Gate;

class CustomerController extends Controller
{
    /**
     * Display a listing of customer records with optional search filter.
     */
    public function index(Request $request): AnonymousResourceCollection
    {
        Gate::authorize('viewAny', Customer::class);

        $searchTerm = $request->query('search') ?? $request->query('name');

        $customers = Customer::query()
            ->when($searchTerm, fn ($query) => $query->search($searchTerm))
            ->latest('id')
            ->paginate($request->query('per_page', 15));

        return CustomerResource::collection($customers);
    }

    /**
     * Store a newly created customer record.
     */
    public function store(StoreCustomerRequest $request): JsonResponse
    {
        $customer = Customer::create($request->validated());

        return (new CustomerResource($customer))
            ->response()
            ->setStatusCode(201);
    }

    /**
     * Display the specified customer record.
     */
    public function show(Customer $customer): CustomerResource
    {
        Gate::authorize('view', $customer);

        return new CustomerResource($customer);
    }

    /**
     * Update the specified customer record.
     */
    public function update(UpdateCustomerRequest $request, Customer $customer): CustomerResource
    {
        $customer->update($request->validated());

        return new CustomerResource($customer);
    }

    /**
     * Remove the specified customer record.
     * Allowed for Owner only.
     */
    public function destroy(Customer $customer): JsonResponse
    {
        Gate::authorize('delete', $customer);

        $customer->delete();

        return response()->json([
            'message' => 'Customer record deleted successfully.',
        ], 200);
    }
}
