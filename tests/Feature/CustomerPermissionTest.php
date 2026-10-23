<?php

namespace Tests\Feature;

use App\Models\Customer;
use App\Models\User;
use Illuminate\Foundation\Testing\DatabaseTransactions;
use Laravel\Sanctum\Sanctum;
use Tests\TestCase;

class CustomerPermissionTest extends TestCase
{
    use DatabaseTransactions;

    protected User $owner;
    protected User $manager;
    protected User $cashier;
    protected Customer $testCustomer;

    protected function setUp(): void
    {
        parent::setUp();

        $this->owner = User::where('email', 'owner@delivergate.lk')->first();
        $this->manager = User::where('email', 'manager@delivergate.lk')->first();
        $this->cashier = User::where('email', 'cashier@delivergate.lk')->first();

        $this->testCustomer = Customer::firstOrCreate(
            ['email' => 'test.customer@example.com'],
            [
                'name' => 'Dilshan Rodrigo',
                'phone' => '+94711122334',
                'address' => '12 Havelock Road, Colombo 05',
                'notes' => 'Frequent buyer',
            ]
        );
    }

    public function test_all_roles_can_view_customers(): void
    {
        foreach ([$this->owner, $this->manager, $this->cashier] as $user) {
            Sanctum::actingAs($user);

            $indexResponse = $this->getJson('/api/customers');
            $indexResponse->assertStatus(200);

            $showResponse = $this->getJson("/api/customers/{$this->testCustomer->id}");
            $showResponse->assertStatus(200);
        }
    }

    public function test_all_roles_can_create_customer(): void
    {
        foreach ([$this->owner, $this->manager, $this->cashier] as $user) {
            Sanctum::actingAs($user);

            $uniqueEmail = 'cust.' . $user->role . '.' . uniqid() . '@example.com';

            $response = $this->postJson('/api/customers', [
                'name' => 'Customer Added by ' . $user->role,
                'email' => $uniqueEmail,
                'phone' => '+94770000000',
                'address' => 'Colombo',
            ]);

            $response->assertStatus(201)
                ->assertJsonPath('data.email', $uniqueEmail);
        }
    }

    public function test_all_roles_can_update_customer(): void
    {
        foreach ([$this->owner, $this->manager, $this->cashier] as $user) {
            Sanctum::actingAs($user);

            $response = $this->putJson("/api/customers/{$this->testCustomer->id}", [
                'notes' => 'Updated by ' . $user->role,
            ]);

            $response->assertStatus(200)
                ->assertJsonPath('data.notes', 'Updated by ' . $user->role);
        }
    }

    public function test_only_owner_can_delete_customer(): void
    {
        // Manager cannot delete
        Sanctum::actingAs($this->manager);
        $this->deleteJson("/api/customers/{$this->testCustomer->id}")
            ->assertStatus(403);

        // Cashier cannot delete
        Sanctum::actingAs($this->cashier);
        $this->deleteJson("/api/customers/{$this->testCustomer->id}")
            ->assertStatus(403);

        // Owner can delete
        Sanctum::actingAs($this->owner);
        $this->deleteJson("/api/customers/{$this->testCustomer->id}")
            ->assertStatus(200)
            ->assertJson(['message' => 'Customer record deleted successfully.']);

        $this->assertDatabaseMissing('customers', ['id' => $this->testCustomer->id]);
    }

    public function test_customer_search_by_keyword(): void
    {
        Sanctum::actingAs($this->owner);

        $response = $this->getJson('/api/customers?search=Dilshan');
        $response->assertStatus(200);

        $names = collect($response->json('data'))->pluck('name');
        $this->assertTrue(
            $names->contains(fn ($name) => str_contains($name, 'Dilshan'))
        );
    }
}
