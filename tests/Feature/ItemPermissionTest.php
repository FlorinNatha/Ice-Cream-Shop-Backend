<?php

namespace Tests\Feature;

use App\Models\Category;
use App\Models\Item;
use App\Models\User;
use Illuminate\Foundation\Testing\DatabaseTransactions;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class ItemPermissionTest extends TestCase
{
    use DatabaseTransactions;

    protected User $owner;
    protected User $manager;
    protected User $cashier;
    protected Item $testItem;
    protected Category $testCategory;

    protected function setUp(): void
    {
        parent::setUp();

        $this->owner = User::where('email', 'owner@delivergate.lk')->first();
        $this->manager = User::where('email', 'manager@delivergate.lk')->first();
        $this->cashier = User::where('email', 'cashier@delivergate.lk')->first();

        $this->testCategory = Category::firstOrCreate(
            ['slug' => 'test-flavor'],
            ['name' => 'Test Flavor', 'description' => 'Test']
        );

        $this->testItem = Item::firstOrCreate(
            ['name' => 'Test Pistachio Gelato'],
            [
                'category_id' => $this->testCategory->id,
                'flavor' => 'Pistachio',
                'price' => 520.00,
                'stock_quantity' => 15,
                'unit' => 'scoop',
                'description' => 'Rich Sicilian Pistachio',
            ]
        );
    }

    public function test_all_roles_can_view_items(): void
    {
        foreach ([$this->owner, $this->manager, $this->cashier] as $user) {
            Sanctum::actingAs($user);

            $indexResponse = $this->getJson('/api/items');
            $indexResponse->assertStatus(200);

            $showResponse = $this->getJson("/api/items/{$this->testItem->id}");
            $showResponse->assertStatus(200);
        }
    }

    public function test_owner_and_manager_can_create_item(): void
    {
        foreach ([$this->owner, $this->manager] as $user) {
            Sanctum::actingAs($user);

            $response = $this->postJson('/api/items', [
                'category_id' => $this->testCategory->id,
                'name' => 'Item By ' . $user->role . ' ' . uniqid(),
                'flavor' => 'Mint Chocolate',
                'price' => 420.00,
                'stock_quantity' => 30,
                'unit' => 'scoop',
            ]);

            $response->assertStatus(201)
                ->assertJsonPath('data.price', fn ($val) => (float) $val == 420);
        }
    }

    public function test_cashier_cannot_create_item(): void
    {
        Sanctum::actingAs($this->cashier);

        $response = $this->postJson('/api/items', [
            'category_id' => $this->testCategory->id,
            'name' => 'Forbidden Cashier Item',
            'flavor' => 'Matcha Green Tea',
            'price' => 600.00,
            'stock_quantity' => 10,
            'unit' => 'scoop',
        ]);

        $response->assertStatus(403);
    }

    public function test_all_roles_can_update_item(): void
    {
        foreach ([$this->owner, $this->manager, $this->cashier] as $user) {
            Sanctum::actingAs($user);

            $response = $this->putJson("/api/items/{$this->testItem->id}", [
                'stock_quantity' => 99,
            ]);

            $response->assertStatus(200)
                ->assertJsonPath('data.stock_quantity', 99);
        }
    }

    public function test_only_owner_can_delete_item(): void
    {
        // Manager cannot delete
        Sanctum::actingAs($this->manager);
        $this->deleteJson("/api/items/{$this->testItem->id}")
            ->assertStatus(403);

        // Cashier cannot delete
        Sanctum::actingAs($this->cashier);
        $this->deleteJson("/api/items/{$this->testItem->id}")
            ->assertStatus(403);

        // Owner can delete
        Sanctum::actingAs($this->owner);
        $this->deleteJson("/api/items/{$this->testItem->id}")
            ->assertStatus(200)
            ->assertJson(['message' => 'Inventory item deleted successfully.']);

        $this->assertDatabaseMissing('items', ['id' => $this->testItem->id]);
    }

    public function test_item_search_by_name(): void
    {
        Sanctum::actingAs($this->owner);

        $response = $this->getJson('/api/items?search=Chocolate');
        $response->assertStatus(200);

        $names = collect($response->json('data'))->pluck('name');
        $this->assertTrue(
            $names->contains(fn ($name) => str_contains(strtolower($name), 'chocolate'))
        );
    }
}
