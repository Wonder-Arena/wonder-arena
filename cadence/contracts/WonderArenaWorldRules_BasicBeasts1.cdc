pub contract WonderArenaWorldRules_BasicBeasts1 {

    pub let AdminStoragePath: StoragePath
    pub let AdminPublicPath: PublicPath
    pub let AdminPrivatePath: PrivatePath

    pub event ContractInitialized()

    pub event StartTimeUpdated(time: UFix64)
    pub event EndTimeUpdated(time: UFix64)

    pub event GroupSizeUpdated(size: UInt8)
    pub event MaxGroupNumberUpdated(number: UInt8)
    pub event MaxChallengeTimesUpdated(times: UInt8)

    pub var startTime: UFix64
    pub var endTime: UFix64

    pub var groupSize: UInt8
    pub var maxGroupNumber: UInt8
    pub var maxChallengeTimes: UInt8

    pub resource Admin {

        pub fun setStartTime(_ time: UFix64) {
            assert(WonderArenaWorldRules_BasicBeasts1.endTime > time, message: "start time should before the end time")
            emit StartTimeUpdated(time: time)
            WonderArenaWorldRules_BasicBeasts1.startTime = time
        }

        pub fun setEndTime(_ time: UFix64) {
            assert(WonderArenaWorldRules_BasicBeasts1.startTime < time, message: "start time should before the end time")
            emit EndTimeUpdated(time: time)
            WonderArenaWorldRules_BasicBeasts1.endTime = time
        }

        pub fun setGroupSize(_ size: UInt8) {
            emit GroupSizeUpdated(size: size)
            WonderArenaWorldRules_BasicBeasts1.groupSize = size
        }

        pub fun setMaxGroupNumber(_ number: UInt8) {
            emit MaxGroupNumberUpdated(number: number)
            WonderArenaWorldRules_BasicBeasts1.maxGroupNumber = number
        }

        pub fun setMaxChallengeTimes(_ times: UInt8) {
            emit MaxChallengeTimesUpdated(times: times)
            WonderArenaWorldRules_BasicBeasts1.maxChallengeTimes = times
        }
    }

    init() {
        self.groupSize = 3
        self.maxGroupNumber = 4
        self.maxChallengeTimes = 3

        let currentBlock = getCurrentBlock()
        self.startTime = currentBlock.timestamp
        self.endTime = currentBlock.timestamp + 100000000.0

        self.AdminStoragePath = /storage/WonderArenaWorldRules_Admin_BasicBeasts1
        self.AdminPublicPath = /public/WonderArenaWorldRules_Admin_BasicBeasts1
        self.AdminPrivatePath = /private/WonderArenaWorldRules_Admin_BasicBeasts1

        self.account.save(<- create Admin(), to: self.AdminStoragePath)

        emit ContractInitialized()
    }
}