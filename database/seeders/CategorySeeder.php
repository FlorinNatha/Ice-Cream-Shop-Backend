<?php

namespace Database\Seeders;

use App\Models\Category;
use Illuminate\Database\Seeder;
use Illuminate\Support\Str;

class CategorySeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $categories = [
            [
                'name' => 'Chocolate',
                'description' => 'Rich and decadent chocolate-based ice cream flavors',
            ],
            [
                'name' => 'Vanilla',
                'description' => 'Classic and aromatic real vanilla bean creations',
            ],
            [
                'name' => 'Strawberry',
                'description' => 'Fresh fruit-infused strawberry flavors and swirls',
            ],
            [
                'name' => 'Mango Sorbet',
                'description' => 'Dairy-free refreshing tropical mango sorbets',
            ],
            [
                'name' => 'Salted Caramel',
                'description' => 'Buttery caramel blended with mineral sea salt',
            ],
        ];

        foreach ($categories as $cat) {
            Category::updateOrCreate(
                ['slug' => Str::slug($cat['name'])],
                [
                    'name' => $cat['name'],
                    'slug' => Str::slug($cat['name']),
                    'description' => $cat['description'],
                ]
            );
        }
    }
}
