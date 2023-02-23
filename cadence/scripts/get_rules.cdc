import WonderArenaWorldRules_BasicBeasts1 from "../contracts/WonderArenaWorldRules_BasicBeasts1.cdc"

pub struct Rule {
    pub let groupSize: UInt8
    pub let maxGroupNumber: UInt8

    init(
        groupSize: UInt8,
        maxGroupNumber: UInt8,
    ) {
        self.groupSize = groupSize
        self.maxGroupNumber = maxGroupNumber
    }
}

pub fun main(): Rule {
    return Rule(
        groupSize: WonderArenaWorldRules_BasicBeasts1.groupSize,
        maxGroupNumber: WonderArenaWorldRules_BasicBeasts1.maxGroupNumber,
    )
}