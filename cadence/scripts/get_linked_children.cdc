import WonderArenaLinkedAccounts_BasicBeasts1 from "../contracts/WonderArenaLinkedAccounts_BasicBeasts1.cdc"

pub fun main(parent: Address): [Address] {
    if let children = WonderArenaLinkedAccounts_BasicBeasts1.parentToChildren[parent] {
        return children.keys
    }
    return []
}