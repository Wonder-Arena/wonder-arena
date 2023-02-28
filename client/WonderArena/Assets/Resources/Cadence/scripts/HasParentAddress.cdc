 import ChildAccount from 0x1b655847a90e644a

pub fun main(address: Address): Bool {
    let tagCap = getAuthAccount(address)
        .getCapability<&ChildAccount.ChildAccountTag>(ChildAccount.ChildAccountTagPrivatePath)
        .borrow()

    if let cap = tagCap {
        if let parent = cap.parentAddress {
            return true
        }
    }

    return false
}