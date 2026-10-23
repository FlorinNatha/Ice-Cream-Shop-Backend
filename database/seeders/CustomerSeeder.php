<?php

namespace Database\Seeders;

use App\Models\Customer;
use Illuminate\Database\Seeder;

class CustomerSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $customers = [
            [
                'name' => 'John Perera',
                'email' => 'john.perera@example.com',
                'phone' => '+94771234567',
                'address' => '42 Galle Road, Colombo 03',
                'notes' => 'Prefers dark chocolate and waffle cones',
            ],
            [
                'name' => 'Sarah Fernando',
                'email' => 'sarah.fernando@example.com',
                'phone' => '+94719876543',
                'address' => '15 Flower Road, Colombo 07',
                'notes' => 'Allergic to nuts; loves strawberry cheesecake',
            ],
            [
                'name' => 'Kamal Silva',
                'email' => 'kamal.silva@example.com',
                'phone' => '+94765554321',
                'address' => '88 Kandy Road, Kadawatha',
                'notes' => 'Regular weekend customer for party tubs',
            ],
        ];

        foreach ($customers as $cust) {
            Customer::updateOrCreate(
                ['email' => $cust['email']],
                $cust
            );
        }
    }
}
