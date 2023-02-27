import ChildAccount from "../contracts/core/ChildAccount.cdc"

transaction() {
    prepare(signer: AuthAccount) {
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
    }

    execute {}
}