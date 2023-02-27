import ChildAccount from "../contracts/core/ChildAccount.cdc"

pub fun main(address: Address): [Address] {
    let manager = getAuthAccount(address)
        .borrow<&ChildAccount.ChildAccountManager>(from: ChildAccount.ChildAccountManagerStoragePath)
        ?? panic("borrow manager failed")

    return manager.getChildAccountAddresses()
}