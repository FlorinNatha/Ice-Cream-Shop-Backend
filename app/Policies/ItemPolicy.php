<?php

namespace App\Policies;

use App\Models\Item;
use App\Models\User;

class ItemPolicy
{
    /**
     * Determine whether the user can view any items.
     * Allowed: Owner, Manager, Cashier.
     */
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [User::ROLE_OWNER, User::ROLE_MANAGER, User::ROLE_CASHIER]);
    }

    /**
     * Determine whether the user can view the item.
     * Allowed: Owner, Manager, Cashier.
     */
    public function view(User $user, Item $item): bool
    {
        return in_array($user->role, [User::ROLE_OWNER, User::ROLE_MANAGER, User::ROLE_CASHIER]);
    }

    /**
     * Determine whether the user can create items.
     * Allowed: Owner, Manager (Cashier is NOT allowed to add items).
     */
    public function create(User $user): bool
    {
        return in_array($user->role, [User::ROLE_OWNER, User::ROLE_MANAGER]);
    }

    /**
     * Determine whether the user can update the item.
     * Allowed: Owner, Manager, Cashier.
     */
    public function update(User $user, Item $item): bool
    {
        return in_array($user->role, [User::ROLE_OWNER, User::ROLE_MANAGER, User::ROLE_CASHIER]);
    }

    /**
     * Determine whether the user can delete the item.
     * Allowed: Owner only (Neither Manager nor Cashier can delete items).
     */
    public function delete(User $user, Item $item): bool
    {
        return $user->isOwner();
    }
}
