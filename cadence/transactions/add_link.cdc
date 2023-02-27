import WonderArenaLinkedAccounts_BasicBeasts1 from "../contracts/WonderArenaLinkedAccounts_BasicBeasts1.cdc"

transaction(parent: Address, child: Address) {
    let adminRef: &WonderArenaLinkedAccounts_BasicBeasts1.Admin

    prepare(signer: AuthAccount) {
        self.adminRef = signer
            .borrow<&WonderArenaLinkedAccounts_BasicBeasts1.Admin>(from: WonderArenaLinkedAccounts_BasicBeasts1.AdminStoragePath)
            ?? panic("borrow linked accounts admin failed")
    }

    execute {
        self.adminRef.addLink(parent: parent, child: child)
    }
}