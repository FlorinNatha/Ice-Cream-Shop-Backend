
<?php

namespace App\Policies;

use App\Models\Customer;
use App\Models\User;

class CustomerPolicy
{
    /**
     * Determine whether the user can view any customers.
     * Allowed: Owner, Manager, Cashier.
     */
    public function viewAny(User $user): bool
    {
        return in_array($user->role, [User::ROLE_OWNER, User::ROLE_MANAGER, User::ROLE_CASHIER]);
    }

    /**
     * Determine whether the user can view the customer.
     * Allowed: Owner, Manager, Cashier.
     */
    public function view(User $user, Customer $customer): bool
    {
        return in_array($user->role, [User::ROLE_OWNER, User::ROLE_MANAGER, User::ROLE_CASHIER]);
    }

    /**
     * Determine whether the user can create customer records.
     * Allowed: Owner, Manager, Cashier.
     */
    public function create(User $user): bool
    {
        return in_array($user->role, [User::ROLE_OWNER, User::ROLE_MANAGER, User::ROLE_CASHIER]);
    }

    /**
     * Determine whether the user can update the customer record.
     * Allowed: Owner, Manager, Cashier.
     */
    public function update(User $user, Customer $customer): bool
    {
        return in_array($user->role, [User::ROLE_OWNER, User::ROLE_MANAGER, User::ROLE_CASHIER]);
    }

    /**
     * Determine whether the user can delete the customer record.
     * Allowed: Owner only (Neither Manager nor Cashier can delete customers).
     */
    public function delete(User $user, Customer $customer): bool
    {
        return $user->isOwner();
    }
}
