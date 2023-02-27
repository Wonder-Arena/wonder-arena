import ChildAccount from "../contracts/core/ChildAccount.cdc"
import MetadataViews from "../contracts/core/MetadataViews.cdc"

/// Signing account claims a Capability to specified Address's AuthAccount
/// and adds it as a child account in its ChildAccountManager, allowing it 
/// to maintain the claimed Capability
/// Note that this transaction assumes we're linking an account created by a
/// ChildAccountCreator and the child account already has a ChildAccountTag.
///
transaction(
    pubKey: String,
    childAddress: Address
  ) {
  let managerRef: &ChildAccount.ChildAccountManager
  let info: ChildAccount.ChildAccountInfo
  let childAccountCap: Capability<&AuthAccount>

  prepare(signer: AuthAccount) {
    let childAccountName = "WonderArena ".concat(childAddress.toString())
    let childAccountDescription = childAccountName
    let clientIconURL = ""
    let clientExternalURL = ""

    // Get ChildAccountManager Capability, linking if necessary
    if signer.borrow<
        &ChildAccount.ChildAccountManager
      >(
        from: ChildAccount.ChildAccountManagerStoragePath
      ) == nil {
      // Save a ChildAccountManager to the signer's account
      signer.save(
        <-ChildAccount.createChildAccountManager(),
        to: ChildAccount.ChildAccountManagerStoragePath
      )
    }
    // Ensure ChildAccountManagerViewer is linked properly
    if !signer.getCapability<
        &ChildAccount.ChildAccountManager{ChildAccount.ChildAccountManagerViewer}
      >(ChildAccount.ChildAccountManagerPublicPath).check() {
      // Link
      signer.link<
        &ChildAccount.ChildAccountManager{ChildAccount.ChildAccountManagerViewer}
      >(
        ChildAccount.ChildAccountManagerPublicPath,
        target: ChildAccount.ChildAccountManagerStoragePath
      )
    }
    // Get ChildAccountManager reference from signer
    self.managerRef = signer.borrow<
        &ChildAccount.ChildAccountManager
      >(from: ChildAccount.ChildAccountManagerStoragePath)!
    // Claim the previously published AuthAccount Capability from the given Address
    self.childAccountCap = signer.inbox.claim<&AuthAccount>(
        "AuthAccountCapability",
        provider: childAddress
      ) ?? panic(
        "No AuthAccount Capability available from given provider"
        .concat(childAddress.toString())
        .concat(" with name ")
        .concat("AuthAccountCapability")
      )
    // Construct ChildAccountInfo struct from given arguments
    self.info = ChildAccount.ChildAccountInfo(
      name: childAccountName,
      description: childAccountDescription,
      clientIconURL: MetadataViews.HTTPFile(url: clientIconURL),
      clienExternalURL: MetadataViews.ExternalURL(clientExternalURL),
      originatingPublicKey: pubKey
    )
  }

  execute {
    // Add account as child to the ChildAccountManager
    self.managerRef.addAsChildAccount(childAccountCap: self.childAccountCap, childAccountInfo: self.info)
  }
}