// 例：ユーザー権限システムの設計検証
abstract sig Bool {}
one sig True, False extends Bool {}

sig User { 
  permissions: set Permission,
  isDeleted: one Bool
}
sig Admin extends User {}
sig Permission {}

// 削除権限を定義
one sig DeletePermission extends Permission {}

fact AdminHasAllPermissions {
    all a: Admin | a.permissions = Permission
}

pred canDelete[admin: Admin, target: User] {
  DeletePermission in admin.permissions
}

pred deleteAllUsers[admin: Admin] {
  all u: User | u != admin implies u.isDeleted = True
}

// この設計には論理的矛盾がないか？
assert NoAdminSelfDeletion {
  all admin: Admin | 
    deleteAllUsers[admin] implies admin.isDeleted = False
}

run {} for 5
check NoAdminSelfDeletion for 5