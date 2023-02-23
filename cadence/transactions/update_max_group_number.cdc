import WonderArenaWorldRules_BasicBeasts1 from "../../contracts/WonderArenaWorldRules_BasicBeasts1.cdc"

transaction(maxGroupNumber: UInt8) {
    let adminRef: &WonderArenaWorldRules_BasicBeasts1.Admin
    prepare(acct: AuthAccount) {
        self.adminRef = acct
            .borrow<&WonderArenaWorldRules_BasicBeasts1.Admin>(from: WonderArenaWorldRules_BasicBeasts1.AdminStoragePath)
            ?? panic("Could not borrow admin")
    }

    execute {
        self.adminRef.setMaxGroupNumber(maxGroupNumber)
    }
}
