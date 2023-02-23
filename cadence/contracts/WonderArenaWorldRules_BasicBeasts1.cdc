pub contract WonderArenaWorldRules_BasicBeasts1 {

    pub let AdminStoragePath: StoragePath
    pub let AdminPublicPath: PublicPath
    pub let AdminPrivatePath: PrivatePath

    pub event ContractInitialized()

    pub event GroupSizeUpdated(size: UInt8)
    pub event MaxGroupNumberUpdated(number: UInt8)

    pub var groupSize: UInt8
    pub var maxGroupNumber: UInt8

    pub resource Admin {

        pub fun setGroupSize(_ size: UInt8) {
            emit GroupSizeUpdated(size: size)
            WonderArenaWorldRules_BasicBeasts1.groupSize = size
        }

        pub fun setMaxGroupNumber(_ number: UInt8) {
            emit MaxGroupNumberUpdated(number: number)
            WonderArenaWorldRules_BasicBeasts1.maxGroupNumber = number
        }
    }

    init() {
        self.groupSize = 3
        self.maxGroupNumber = 10

        self.AdminStoragePath = /storage/WonderArenaWorldRules_Admin_BasicBeasts1
        self.AdminPublicPath = /public/WonderArenaWorldRules_Admin_BasicBeasts1
        self.AdminPrivatePath = /private/WonderArenaWorldRules_Admin_BasicBeasts1

        self.account.save(<- create Admin(), to: self.AdminStoragePath)

        emit ContractInitialized()
    }
}