<?php

namespace Database\Seeders;

use App\Models\User;
use Illuminate\Database\Seeder;
use Illuminate\Support\Facades\Hash;

class UserSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $users = [
            [
                'name' => 'Shop Owner',
                'email' => 'owner@delivergate.lk',
                'password' => Hash::make('password123'),
                'role' => User::ROLE_OWNER,
            ],
            [
                'name' => 'Store Manager',
                'email' => 'manager@delivergate.lk',
                'password' => Hash::make('password123'),
                'role' => User::ROLE_MANAGER,
            ],
            [
                'name' => 'Front Cashier',
                'email' => 'cashier@delivergate.lk',
                'password' => Hash::make('password123'),
                'role' => User::ROLE_CASHIER,
            ],
        ];

        foreach ($users as $userData) {
            User::updateOrCreate(
                ['email' => $userData['email']],
                $userData
            );
        }
    }
}
