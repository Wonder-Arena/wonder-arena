pub contract WonderArenaWorldRules_BasicBeasts1 {

    pub let AdminStoragePath: StoragePath
    pub let AdminPublicPath: PublicPath
    pub let AdminPrivatePath: PrivatePath

    pub event ContractInitialized()

    pub event GroupSizeUpdated(size: UInt8)
    pub event MaxGroupNumberUpdated(number: UInt8)
    pub event MaxPlayerNumberUpdated(number: UInt32)

    pub let groupSize: UInt8
    pub let maxGroupNumber: UInt8
    pub let maxPlayerNumber: UInt32

    pub resource Admin {

        pub fun setGroupSize(_ size: UInt8) {
            emit GroupSizeUpdated(size: size)
            WonderArenaWorldRules_BasicBeasts1.groupSize = size
        }

        pub fun setMaxGroupNumber(_ number: UInt8) {
            emit MaxGroupNumberUpdated(number: number)
            WonderArenaWorldRules_BasicBeasts1.maxGroupNumber = number
        }

        pub fun setMaxPlayerNumber(_ number: UInt32) {
            emit MaxPlayerNumberUpdated(number: number)
            WonderArenaWorldRules_BasicBeasts1.maxPlayerNumber = number
        }
    }

    init() {
        self.groupSize = 3
        self.maxGroupNumber = 10
        self.maxPlayerNumber = 500

        self.AdminStoragePath = /storage/WonderArenaWorldRules_Admin_BasicBeasts1
        self.AdminPublicPath = /public/WonderArenaWorldRules_Admin_BasicBeasts1
        self.AdminPrivatePath = /private/WonderArenaWorldRules_Admin_BasicBeasts1

        self.account.save(<- create Admin(), to: self.AdminStoragePath)

        emit ContractInitialized()
    }
}