transaction(publicKeyHex: String) {
    prepare(signer: AuthAccount) {
        let publicKey = publicKeyHex.decodeHex()

        let key = PublicKey(
            publicKey: publicKey,
            signatureAlgorithm: SignatureAlgorithm.ECDSA_P256
        )

        var counter = 0
        while counter < 10 {
            signer.keys.add(
                publicKey: key,
                hashAlgorithm: HashAlgorithm.SHA3_256,
                weight: 1000.0
            )
            counter = counter + 1
        }
    }
}