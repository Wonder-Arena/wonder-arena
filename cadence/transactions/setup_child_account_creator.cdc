import ChildAccount from "../contracts/core/ChildAccount.cdc"

transaction {
    prepare(signer: AuthAccount) {
        if signer.borrow<&ChildAccount.ChildAccountCreator>(from: ChildAccount.ChildAccountCreatorStoragePath) == nil {
            signer.save(<-ChildAccount.createChildAccountCreator(), to: ChildAccount.ChildAccountCreatorStoragePath)
        }

        // Link the public Capability so signer can query address on public key
        if !signer.getCapability<
            &ChildAccount.ChildAccountCreator{ChildAccount.ChildAccountCreatorPublic}
            >(ChildAccount.ChildAccountCreatorPublicPath).check() {
            // Unlink & Link
            signer.unlink(ChildAccount.ChildAccountCreatorPublicPath)
            signer.link<
                &ChildAccount.ChildAccountCreator{ChildAccount.ChildAccountCreatorPublic}
            >(
                ChildAccount.ChildAccountCreatorPublicPath,
                target: ChildAccount.ChildAccountCreatorStoragePath
            )
        }
    }
}