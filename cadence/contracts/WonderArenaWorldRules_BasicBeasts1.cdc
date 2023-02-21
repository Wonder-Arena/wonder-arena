pub contract WonderArenaWorldRules_BasicBeasts1 {
    pub let groupSize: UInt8
    pub let maxGroupNumber: UInt8

    pub let maxPlayerNumber: UInt8
    pub let winnerThreshold: UInt8

    init() {
        self.groupSize = 3
        self.maxGroupNumber = 3
        self.maxPlayerNumber = 10
        self.winnerThreshold = 6
    }
}