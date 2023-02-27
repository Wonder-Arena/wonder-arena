pub contract WonderArenaLinkedAccounts_BasicBeasts1 {
    pub let AdminStoragePath: StoragePath
    pub let AdminPublicPath: PublicPath
    pub let AdminPrivatePath: PrivatePath

    pub event ContractInitialized()

    // parent account => child accounts
    pub let parentToChildren: {Address: {Address: Bool}}

    pub resource Admin {

        pub fun addLink(parent: Address, child: Address) {
            var children: {Address: Bool} = {}
            if let _children = WonderArenaLinkedAccounts_BasicBeasts1.parentToChildren[parent] {
                children = _children 
                if let c = _children[child] {
                    // already exists, do nothing
                } else {
                    children.insert(key: child, true)
                }
            } else {
                children.insert(key: child, true) 
            }
            WonderArenaLinkedAccounts_BasicBeasts1.parentToChildren.insert(key: parent, children)
        }
    }

    init() {
        self.parentToChildren = {}

        self.AdminStoragePath = /storage/WonderArenaLinkedAccounts_Admin_BasicBeasts1
        self.AdminPublicPath = /public/WonderArenaLinkedAccounts_Admin_BasicBeasts1
        self.AdminPrivatePath = /private/WonderArenaLinkedAccounts_Admin_BasicBeasts1

        self.account.save(<- create Admin(), to: self.AdminStoragePath)

        emit ContractInitialized()
    }
}