<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\CustomerController;
use App\Http\Controllers\Api\ItemController;
use App\Models\Category;
use Illuminate\Support\Facades\Route;

/*
|--------------------------------------------------------------------------
| API Routes
|--------------------------------------------------------------------------
| Delivergate Ice Cream Shop API Endpoints
*/

// Authentication Endpoints
Route::prefix('auth')->group(function () {
    Route::post('/login', [AuthController::class, 'login']);
    Route::post('/logout', [AuthController::class, 'logout'])->middleware('auth:sanctum');
    Route::get('/me', [AuthController::class, 'me'])->middleware('auth:sanctum');
});

// Protected Endpoints
Route::middleware('auth:sanctum')->group(function () {
    // Inventory Items
    Route::apiResource('items', ItemController::class);

    // Customer Records
    Route::apiResource('customers', CustomerController::class);

    // Flavor Categories helper endpoint
    Route::get('categories', function () {
        return response()->json([
            'data' => Category::all(['id', 'name', 'slug', 'description']),
        ]);
    });
});
