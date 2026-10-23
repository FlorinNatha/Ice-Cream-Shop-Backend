<?php

namespace Database\Seeders;

use App\Models\Category;
use App\Models\Item;
use Illuminate\Database\Seeder;

class ItemSeeder extends Seeder
{
    /**
     * Run the database seeds.
     */
    public function run(): void
    {
        $chocolate = Category::where('slug', 'chocolate')->first();
        $vanilla = Category::where('slug', 'vanilla')->first();
        $strawberry = Category::where('slug', 'strawberry')->first();
        $mango = Category::where('slug', 'mango-sorbet')->first();
        $caramel = Category::where('slug', 'salted-caramel')->first();

        $items = [
            [
                'category_id' => $chocolate?->id,
                'name' => 'Belgian Dark Chocolate Scoop',
                'flavor' => 'Dark Chocolate 70%',
                'price' => 450.00,
                'stock_quantity' => 80,
                'unit' => 'scoop',
                'description' => 'Made from premium single-origin Belgian cocoa beans.',
            ],
            [
                'category_id' => $chocolate?->id,
                'name' => 'Chocolate Fudge Brownie Tub (500ml)',
                'flavor' => 'Fudge Brownie',
                'price' => 1350.00,
                'stock_quantity' => 25,
                'unit' => 'tub',
                'description' => 'Decadent chocolate swirl with chewy brownie chunks.',
            ],
            [
                'category_id' => $vanilla?->id,
                'name' => 'Madagascar Vanilla Bean Scoop',
                'flavor' => 'Vanilla Bean',
                'price' => 380.00,
                'stock_quantity' => 100,
                'unit' => 'scoop',
                'description' => 'Infused with real Bourbon vanilla caviar flecks.',
            ],
            [
                'category_id' => $strawberry?->id,
                'name' => 'Wild Strawberry Cheesecake Scoop',
                'flavor' => 'Strawberry Cheesecake',
                'price' => 480.00,
                'stock_quantity' => 60,
                'unit' => 'scoop',
                'description' => 'Creamy cream cheese ice cream with fresh strawberry coulis.',
            ],
            [
                'category_id' => $mango?->id,
                'name' => 'Alphonso Mango Sorbet Scoop',
                'flavor' => 'Alphonso Mango',
                'price' => 400.00,
                'stock_quantity' => 50,
                'unit' => 'scoop',
                'description' => '100% vegan tropical sorbet made with sun-ripened Alphonso mangoes.',
            ],
            [
                'category_id' => $caramel?->id,
                'name' => 'Sea Salt Caramel Crunch Tub (500ml)',
                'flavor' => 'Salted Caramel',
                'price' => 1400.00,
                'stock_quantity' => 30,
                'unit' => 'tub',
                'description' => 'Golden caramel ribbon with toasted honeycomb crunchies.',
            ],
        ];

        foreach ($items as $itemData) {
            Item::updateOrCreate(
                ['name' => $itemData['name']],
                $itemData
            );
        }
    }
}
