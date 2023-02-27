pub contract WonderArenaLinkedAccounts_BasicBeasts1 {

    // parent account => child accounts
    pub let parentToChildren: {Address: {Address: Bool}}
    // child account => parent accounts
    pub let childToParents: {Address: {Address: Bool}}

    pub resource Admin {

        pub fun addAccountPair(parent: Address, child: Address, childPublicKeyHex: String) {
            var children: {Address: Bool} = {}
            if let _children = self.parentToChildren[parent] {
                children = _children 
                if let c = _children[child] {
                    // already exists, do nothing
                } else {
                    children.insert(key: child, true)
                }
            } else {
                children.insert(child, true) 
            }
            self.parentToChildren.insert(key: parent, children)

            var parents: {Address: Bool} = {}
            if let _parents = self.childToParents[parent] {

            }

            self.childToParent.insert(key: child, {parent: true})
        }

    }

}