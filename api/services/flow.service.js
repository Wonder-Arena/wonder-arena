const fcl = require('@onflow/fcl')
const utils = require('../utils/flow')
const { PrismaClient } = require('@prisma/client')
const createError = require('http-errors')
const prisma = new PrismaClient()
const CryptoJS = require("crypto-js");

require('dotenv').config()
utils.switchToTestnet()

const WonderArenaPath = "0xWonderArena"
const WonderArenaAddress = process.env.ADMIN_ADDRESS

class flowService {
  static encryptPrivateKey(key) {
    const secret = process.env.SECRET_PASSPHRASE
    const encrypted = CryptoJS.AES.encrypt(key, secret).toString()
    return encrypted
  }

  static decryptPrivateKey(encrypted) {
    const secret = process.env.SECRET_PASSPHRASE
    const decrypted = CryptoJS.AES.decrypt(encrypted, secret).toString(CryptoJS.enc.Utf8)
    return decrypted
  }

  static async getAdminAccountWithKeyIndex(keyIndex) {
    const FlowSigner = (await import('../utils/signer.mjs')).default
    const key = this.decryptPrivateKey(process.env.ADMIN_ENCRYPTED_PRIVATE_KEY)

    const signer = new FlowSigner(
      process.env.ADMIN_ADDRESS,
      key,
      keyIndex,
      {}
    )
    return signer
  }

  static async getDroperWithKeyIndex(keyIndex) {
    const FlowSigner = (await import('../utils/signer.mjs')).default
    const key = this.decryptPrivateKey(process.env.DROPER_ENCRYPTED_PRIVATE_KEY)

    const signer = new FlowSigner(
      process.env.DROPER_ADDRESS,
      key,
      keyIndex,
      {}
    )
    return signer
  }

  static async getUserSigner(flowAccount) {
    const FlowSigner = (await import('../utils/signer.mjs')).default
    const privateKey = this.decryptPrivateKey(flowAccount.encryptedPrivateKey)
    const signer = new FlowSigner(
      flowAccount.address,
      privateKey,
      0,
      {}
    )

    return signer
  }

  static generateKeypair() {
    const EC = require("elliptic").ec
    const ec = new EC("p256")

    let keypair = ec.genKeyPair()
    let privateKey = keypair.getPrivate().toString('hex')
    while (privateKey.length != 64) {
      keypair = ec.genKeyPair()
      privateKey = keypair.getPrivate().toString('hex')
    }

    const publicKey = keypair.getPublic().encode('hex').substring(2)
    return { privateKey: privateKey, publicKey: publicKey }
  }


  static AdminKeys = {
    0: false,
    1: false,
    2: false,
    3: false,
    4: false,
    5: false,
    6: false,
    7: false,
    8: false,
    9: false,
    10: false
  }

  static DroperKeys = {
    0: false,
    1: false,
    2: false,
    3: false,
    4: false,
    5: false,
    6: false,
    7: false,
    8: false,
    9: false,
    10: false
  }

  static async setGeneratorIndex() {
    const users = await prisma.user.findMany({
      where: { flowAccount: null, generatorIndex: null }
    })

    for (let i = 0; i < users.length; i++) {
      const user = users[i]
      const indicies = Object.keys(this.AdminKeys)
      const index = user.id % indicies.length
      try {
        await prisma.user.update({
          where: { email: user.email },
          data: { generatorIndex: index }
        }) 
      } catch (e) {
        console.log(e)
      }
    } 
  }

  static async generateWonderArenaAccounts() {
    await this.setGeneratorIndex()
    const indicies = Object.keys(this.AdminKeys) 
    for (let i = 0; i < indicies.length; i++) {
      let index = indicies[i]
      if (this.AdminKeys[index] == false) {
        this.generateWonderArenaAccountsWithKey(index)
      }
    }
  }

  static async generateWonderArenaAccountsWithKey(_keyIndex) {
    if (this.AdminKeys[_keyIndex] == true) {
      return
    }
    this.AdminKeys[_keyIndex] = true
    const keyIndex = parseInt(_keyIndex)
    const users = await prisma.user.findMany({
      where: { flowAccount: null, generatorIndex: keyIndex }
    })
    
    for (let i = 0; i < users.length; i++) {
      let user = users[i]
      try {
        console.log(`[${user.name}] Generating address with key #${keyIndex}`)
        await this.createWonderArenaAccount(user, keyIndex)
        console.log(`[${user.name}] Address is generated with key #${keyIndex}`)
      } catch (e) {
        console.log(e)
      }
    }

    this.AdminKeys[_keyIndex] = false
  }

  static async createWonderArenaAccount(data, keyIndex) {
    const { name, email } = data
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (user.flowAccount) {
      delete user.flowAccount.id
      delete user.flowAccount.encryptedPrivateKey
      delete user.flowAccount.userId
      return user.flowAccount
    }

    const signer = await this.getAdminAccountWithKeyIndex(keyIndex)

    const { privateKey: privateKey, publicKey: publicKeyHex } = this.generateKeypair()
    const code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena
    import FungibleToken from 0x9a0766d93b6608b7
    import NonFungibleToken from 0x631e88ae7f1d7c20
    import MetadataViews from 0x631e88ae7f1d7c20
    import BasicBeasts from 0xfa252d0aa22bf86a
    import ChildAccount from 0x1b655847a90e644a
    
    transaction(name: String, publicKeyHex: String) {
        prepare(signer: AuthAccount) {
            if signer.borrow<&ChildAccount.ChildAccountCreator>(from: ChildAccount.ChildAccountCreatorStoragePath) == nil {
              signer.save(<-ChildAccount.createChildAccountCreator(), to: ChildAccount.ChildAccountCreatorStoragePath)
            }

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

            let creatorRef = signer.borrow<&ChildAccount.ChildAccountCreator>(
                from: ChildAccount.ChildAccountCreatorStoragePath
            ) ?? panic("Problem getting a ChildAccountCreator reference!")

            let info = ChildAccount.ChildAccountInfo(
                name: name,
                description: "WonderArena account for ".concat(name),
                clientIconURL: MetadataViews.HTTPFile(url: ""),
                clienExternalURL: MetadataViews.ExternalURL(""),
                originatingPublicKey: publicKeyHex
            )

            let account: AuthAccount = creatorRef.createChildAccount(
                signer: signer,
                initialFundingAmount: 1.0,
                childAccountInfo: info
            )
    
            // setup WonderArena player
            let player <- WonderArenaBattleField_BasicBeasts1.createNewPlayer(
                name: name,
                address: account.address
            )
    
            account.save(<- player, to: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
            let playerCap = account.link<&WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}>(
                WonderArenaBattleField_BasicBeasts1.PlayerPublicPath, 
                target: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
                ?? panic("link player failed")

            let adminRef = signer
                .borrow<&WonderArenaBattleField_BasicBeasts1.Admin>(from: WonderArenaBattleField_BasicBeasts1.AdminStoragePath)
                ?? panic("borrow battle field admin failed")
    
            adminRef.register(playerCap: playerCap)
    
            // setup BasicBeasts collection
            account.save(<-BasicBeasts.createEmptyCollection(), to: BasicBeasts.CollectionStoragePath)
            account.unlink(BasicBeasts.CollectionPublicPath)
            account.link<&BasicBeasts.Collection{NonFungibleToken.Receiver, NonFungibleToken.CollectionPublic, MetadataViews.ResolverCollection, BasicBeasts.BeastCollectionPublic}>(BasicBeasts.CollectionPublicPath, target: BasicBeasts.CollectionStoragePath)
        }
    }
    `
      .replaceAll(WonderArenaPath, WonderArenaAddress)

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(name, t.String),
        arg(publicKeyHex, t.String)
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        let event = tx.events.find((e) => e.type == 'flow.AccountCreated')
        if (!event) {
          throw { statusCode: 500, message: "Account generation failed" }
        }
        const address = event.data.address
        let flowAccount = await prisma.flowAccount.create({
          data: {
            address: address,
            encryptedPrivateKey: this.encryptPrivateKey(privateKey),
            userId: user.id
          }
        })

        delete flowAccount.id
        delete flowAccount.encryptedPrivateKey
        delete flowAccount.userId
        delete flowAccount.createdAt
        delete flowAccount.updatedAt

        return flowAccount
      }
      throw "send transaction failed"
    } catch (e) {
      throw { statusCode: 500, message: `Account generation failed ${e}` }
    }
  }

  static async addDefenderGroup(userData, groupName, beastIDs) {
    const { name, email } = userData
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (!user.flowAccount) {
      throw createError.NotFound('flow account not found')
    }

    let script = `
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena

    pub fun main(address: Address): UInt64 {
        if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
            if let player = playerCap.borrow() {
                return UInt64(player.getDefenderGroups().length)
            }
        }
        return 0
    }
    `
      .replaceAll(WonderArenaPath, WonderArenaAddress)

    const groupNumber = await fcl.query({
      cadence: script,
      args: (arg, t) => [
        arg(user.flowAccount.address, t.Address)
      ]
    })

    if (groupNumber >= 4) {
      throw createError.UnprocessableEntity("Can only have 4 groups at most")
    }

    let signer = await this.getUserSigner(user.flowAccount)
    let code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena

    transaction(name: String, beastIDs: [UInt64]) {
      let playerRef: &WonderArenaBattleField_BasicBeasts1.Player
  
      prepare(acct: AuthAccount) {
          self.playerRef = acct.borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
              ?? panic("borrow player failed")
      }
  
      execute {
          let group = WonderArenaBattleField_BasicBeasts1.BeastGroup(
              name: name,
              beastIDs: beastIDs
          )
          self.playerRef.addDefenderGroup(group: group)
      }
    }
    `
      .replaceAll(WonderArenaPath, WonderArenaAddress)

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(groupName, t.String),
        arg(beastIDs.map((id) => id.toString()), t.Array(t.UInt64))
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        if (tx.status === 4 && tx.statusCode === 0) {
          return
        }
      }
      throw "send transaction failed"
    } catch (e) {
      throw { statusCode: 500, message: `Add defender group failed ${e}` }
    }
  }

  static async removeDefenderGroup(userData, groupName) {
    const { name, email } = userData
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (!user.flowAccount) {
      throw createError.NotFound('flow account not found')
    }

    let signer = await this.getUserSigner(user.flowAccount)
    let code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena

    transaction(name: String) {
      let playerRef: &WonderArenaBattleField_BasicBeasts1.Player
  
      prepare(acct: AuthAccount) {
          self.playerRef = acct.borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
              ?? panic("borrow player failed")
      }
  
      execute {
          self.playerRef.removeDefenderGroup(name: name)
      }
    }
    `
      .replaceAll(WonderArenaPath, WonderArenaAddress)

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(groupName, t.String)
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        if (tx.status === 4 && tx.statusCode === 0) {
          return
        }
      }
      throw "send transaction failed"
    } catch (e) {
      throw { statusCode: 500, message: `remove defender group failed ${e}` }
    }
  }

  static async claimBBs(userData) {
    const { name, email } = userData
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (!user.flowAccount) {
      throw createError.NotFound('flow account not found')
    }

    if (user.claimedBBs) {
      throw createError.UnprocessableEntity('already claimed')
    }

    let keyIndex = null
    for (const [key, value] of Object.entries(this.DroperKeys)) {
      if (value == false) {
        keyIndex = parseInt(key)
        break
      }
    } 

    if (keyIndex == null) {
      throw createError.InternalServerError("Server is busy")
    }

    this.DroperKeys[keyIndex] = true
    let signer = await this.getDroperWithKeyIndex(keyIndex)
    let code = `
    import NonFungibleToken from 0x631e88ae7f1d7c20
    import BasicBeasts from 0xfa252d0aa22bf86a
    
    transaction(recipient: Address) {
        let senderCollection: &BasicBeasts.Collection
        let recipientCollection: &BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}
    
        prepare(acct: AuthAccount) {
            self.senderCollection = acct.borrow<&BasicBeasts.Collection>(from: BasicBeasts.CollectionStoragePath)
                ?? panic("borrow sender collection failed")
    
            self.recipientCollection = getAccount(recipient)
                .getCapability(BasicBeasts.CollectionPublicPath)
                .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
                ?? panic("borrow recipient collection failed")
        }
    
        execute {
          let ids = self.senderCollection.getIDs()
          assert(ids.length >= 3, message: "Basic Beasts are not enough")
          let tokenIDs = [ids[0], ids[1], ids[2]]

          for id in tokenIDs {
              let beast <- self.senderCollection.withdraw(withdrawID: id)
              self.recipientCollection.deposit(token: <- beast)
          }
        }
    }
    `

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(user.flowAccount.address, t.Address)
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        this.DroperKeys[keyIndex] = false
        if (tx.status === 4 && tx.statusCode === 0) {
          let updatedUser = await prisma.user.update({
            where: { email },
            data: { claimedBBs: true }
          })
          return updatedUser
        }
      }
      throw "send transaction failed"
    } catch (e) {
      this.DroperKeys[keyIndex] = false
      throw { statusCode: 500, message: `Claim BasicBeasts failed ${e}` }
    }
  }

  static async buyBB(userData, tokenID) {
    const { name, email } = userData
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (!user.flowAccount) {
      throw createError.NotFound('flow account not found')
    }

    let keyIndex = null
    for (const [key, value] of Object.entries(this.DroperKeys)) {
      if (value == false) {
        keyIndex = parseInt(key)
        break
      }
    } 

    if (keyIndex == null) {
      throw createError.InternalServerError("Server is busy")
    }

    this.DroperKeys[keyIndex] = true
    let signer = await this.getDroperWithKeyIndex(keyIndex)
    let code = `
    import NonFungibleToken from 0x631e88ae7f1d7c20
    import BasicBeasts from 0xfa252d0aa22bf86a
    
    transaction(recipient: Address, tokenID: UInt64) {
        let senderCollection: &BasicBeasts.Collection
        let recipientCollection: &BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}
    
        prepare(acct: AuthAccount) {
            self.senderCollection = acct.borrow<&BasicBeasts.Collection>(from: BasicBeasts.CollectionStoragePath)
                ?? panic("borrow sender collection failed")
    
            self.recipientCollection = getAccount(recipient)
                .getCapability(BasicBeasts.CollectionPublicPath)
                .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
                ?? panic("borrow recipient collection failed")
        }
    
        execute {
          if let beast = self.senderCollection.borrowBeast(id: tokenID) {
              let beast <- self.senderCollection.withdraw(withdrawID: tokenID)
              self.recipientCollection.deposit(token: <- beast)
          }
        }
    }
    `

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(user.flowAccount.address, t.Address),
        arg(tokenID.toString(), t.UInt64)
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        this.DroperKeys[keyIndex] = false
        if (tx.status === 4 && tx.statusCode === 0) {
          return
        }
      }
      throw "send transaction failed"
    } catch (e) {
      this.DroperKeys[keyIndex] = false
      throw createError.InternalServerError(`Buy BasicBeast failed ${e}`)
    }
  }


  static async fight(userData, attackerIDs, defenderAddress) {
    const { name, email } = userData
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (!user.flowAccount) {
      throw createError.NotFound('flow account not found')
    }

    const defenderAccount = await prisma.flowAccount.findUnique({
      where: { address: defenderAddress },
      include: { user: true }
    })

    if (!defenderAccount.user) {
      throw createError.NotFound('defender not found')
    }

    if (defenderAccount.user.id == user.id) {
      throw createError.UnprocessableEntity('attacker and defender should not be the same')
    }

    let script = `
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena

    pub fun main(attacker: Address, defender: Address): UInt64 {
        if let records = WonderArenaBattleField_BasicBeasts1.attackerChallenges[attacker] {
            if let innerRecords = records[defender] {
                return UInt64(innerRecords.keys.length)
            }
        }
        return 0
    }
    `
      .replaceAll(WonderArenaPath, WonderArenaAddress)

    const challengeTimes = await fcl.query({
      cadence: script,
      args: (arg, t) => [
        arg(user.flowAccount.address, t.Address),
        arg(defenderAddress, t.Address)
      ]
    })

    // TODO:
    if (challengeTimes >= 100) {
      throw createError.UnprocessableEntity("Can only challenge a player for 3 times at most")
    }

    let keyIndex = null
    for (const [key, value] of Object.entries(this.AdminKeys)) {
      if (value == false) {
        keyIndex = parseInt(key)
        break
      }
    }

    if (keyIndex == null) {
      throw createError.InternalServerError("Server is busy")
    }

    this.AdminKeys[keyIndex] = true
    let signer = await this.getAdminAccountWithKeyIndex(keyIndex)
    let code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena

    transaction(
      attackerAddress: Address,
      attackerIDs: [UInt64],
      defenderAddress: Address
    ) {
        let adminRef: &WonderArenaBattleField_BasicBeasts1.Admin
        prepare(acct: AuthAccount) {
            self.adminRef = acct
                .borrow<&WonderArenaBattleField_BasicBeasts1.Admin>(from: WonderArenaBattleField_BasicBeasts1.AdminStoragePath)
                ?? panic("borrow battle field admin failed")
        }
    
        execute {
            let attackerGroup = WonderArenaBattleField_BasicBeasts1.BeastGroup(
                name: "AttackerGroup",
                beastIDs: attackerIDs
            )
    
            self.adminRef.fight(
                attackerAddress: attackerAddress,
                attackerGroup: attackerGroup,
                defenderAddress: defenderAddress
            )
        }
    }
    `
      .replaceAll(WonderArenaPath, WonderArenaAddress)

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(user.flowAccount.address, t.Address),
        arg(attackerIDs.map((id) => id.toString()), t.Array(t.UInt64)),
        arg(defenderAddress, t.Address)
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        this.AdminKeys[keyIndex] = false
        if (tx.status === 4 && tx.statusCode === 0) {
          let event = tx.events.find((e) => e.type.includes('ChallengeHappened'))
          if (!event) {
            throw "Fight failed"
          }
          return { attacker: user.flowAccount.address, defender: defenderAddress, challengeUUID: event.data.uuid }
        }
      }
      throw "send transaction failed"
    } catch (e) {
      this.AdminKeys[keyIndex] = false
      throw { statusCode: 500, message: `Fight failed ${e}` }
    }
  }

  static async getPlayer(name, withRecords) {
    const user = await prisma.user.findUnique({
      where: { name },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    delete user.password
    delete user.id
    delete user.createdAt
    delete user.updatedAt
    delete user.generatorIndex

    if (user.flowAccount) {
      if (withRecords) {
        let script = `
        import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena
  
        pub struct PlayerInfo {
          pub let score: Int64 
          pub let attackRecords: {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}}
          pub let defendRecords: {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}}
  
          init(
            score: Int64,
            attackRecords: {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}},
            defendRecords: {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}},
          ) {
            self.score = score
            self.attackRecords = attackRecords
            self.defendRecords = defendRecords
          }
        }
    
        pub fun main(address: Address): PlayerInfo {
            var score: Int64 = 0
            var attackRecords: {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}} = {}
            var defendRecords: {Address: {UInt64: WonderArenaBattleField_BasicBeasts1.ChallengeRecord}} = {}
            if let _attackRecords = WonderArenaBattleField_BasicBeasts1.attackerChallenges[address] {
                attackRecords = _attackRecords
            }
    
            if let _defendRecords = WonderArenaBattleField_BasicBeasts1.defenderChallenges[address] {
                defendRecords = _defendRecords
            }
  
            if let _score = WonderArenaBattleField_BasicBeasts1.scores[address] {
                score = _score
            }
            return PlayerInfo(
              score: score,
              attackRecords: attackRecords,
              defendRecords: defendRecords
            )
        }
        `
        .replaceAll(WonderArenaPath, WonderArenaAddress)

        const playerInfo = await fcl.query({
          cadence: script,
          args: (arg, t) => [
            arg(user.flowAccount.address, t.Address)
          ]
        })

        const attackRecords = Object.values(playerInfo.attackRecords)
          .flatMap((r) => Object.values(r))

        const defendRecords = Object.values(playerInfo.defendRecords)
          .flatMap((r) => Object.values(r))

        const records = attackRecords
          .concat(defendRecords)
          .sort((a, b) => {
            const aID = parseInt(a.id)
            const bID = parseInt(b.id)
            return bID - aID
          })
          .map((r) => {
            delete r.events
            return r
          })

        user.challenges = records
        user.score = parseInt(playerInfo.score)
      }

      delete user.flowAccount.id
      delete user.flowAccount.encryptedPrivateKey
      delete user.flowAccount.userId
      delete user.flowAccount.createdAt
      delete user.flowAccount.updatedAt
    }

    return user
  }

  static async claimReward(userData) {
    const { name, email } = userData
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (!user.flowAccount) {
      throw createError.NotFound('flow account not found')
    }

    let script = `
    import WonderArenaReward_BasicBeasts1 from 0xWonderArena
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena

    pub fun main(host: Address, rewardID: UInt64, claimer: Address): Bool {
        if let rewardCollectionRef = getAccount(host)
            .getCapability(WonderArenaReward_BasicBeasts1.RewardCollectionPublicPath)
            .borrow<&{WonderArenaReward_BasicBeasts1.IRewardCollectionPublic}>() {

            if let reward = rewardCollectionRef.borrowPublicReward(id: rewardID) {
                if let score = WonderArenaBattleField_BasicBeasts1.scores[claimer] {
                    return (score >= reward.scoreThreshold) && (reward.claimed[claimer] == nil)
                }
            }
        }

        return false
    }`
    .replaceAll(WonderArenaPath, WonderArenaAddress)

    const rewardID = process.env.REWARD_ID
    const isEligible = await fcl.query({
      cadence: script,
      args: (arg, t) => [
        arg(WonderArenaAddress, t.Address),
        arg(rewardID, t.UInt64),
        arg(user.flowAccount.address, t.Address)
      ]
    })

    if (!isEligible) {
      throw createError.UnprocessableEntity("not eligible or already claimed")
    }

    let signer = await this.getUserSigner(user.flowAccount)
    let code = `
    import WonderArenaReward_BasicBeasts1 from 0xWonderArena
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena

    transaction(
        host: Address,
        rewardID: UInt64
    ) {
        let rewardCollectionRef: &{WonderArenaReward_BasicBeasts1.IRewardCollectionPublic}
        let playerRef: &WonderArenaBattleField_BasicBeasts1.Player
        prepare(acct: AuthAccount) {
            self.rewardCollectionRef = getAccount(host)
                .getCapability(WonderArenaReward_BasicBeasts1.RewardCollectionPublicPath)
                .borrow<&{WonderArenaReward_BasicBeasts1.IRewardCollectionPublic}>()
                ?? panic("Borrow reward collection failed")

            self.playerRef = acct
                .borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
                ?? panic("Borrow player failed")
        }

        execute {
            let reward = self.rewardCollectionRef.borrowPublicReward(id: rewardID)
                ?? panic("Borrow reward failed")

            reward.claim(player: self.playerRef)
        }
    }
    `
    .replaceAll(WonderArenaPath, WonderArenaAddress)

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(WonderArenaAddress, t.Address),
        arg(rewardID, t.UInt64)
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        if (tx.status === 4 && tx.statusCode === 0) {
          return
        }
      }
      throw "send transaction failed"
    } catch (e) {
      throw createError.InternalServerError(`claim reward failed ${e}`)
    }
  }

  static async accountLink(userData, parentAddress) {
    const { name, email } = userData
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (!user.flowAccount) {
      throw createError.NotFound('flow account not found')
    }

    let script = `
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
    `
    const isLinked = await fcl.query({
      cadence: script,
      args: (arg, t) => [
        arg(user.flowAccount.address, t.Address)
      ]
    })

    if (isLinked) {
      throw createError.UnprocessableEntity("Account is already linked")
    }

    try {
      const accountInfo = await fcl.send([fcl.getAccount(fcl.sansPrefix(parentAddress))])
    } catch (e) {
      throw createError.NotFound('parent address not found on the blockchain')
    }

    let signer = await this.getUserSigner(user.flowAccount)
    let code = `
    import ChildAccount from 0x1b655847a90e644a

    /// Signing account publishes a Capability to its AuthAccount for
    /// the specified parentAddress to claim
    ///
    transaction(parentAddress: Address) {
    
        let authAccountCap: Capability<&AuthAccount>
    
        prepare(signer: AuthAccount) {
            // Get the AuthAccount Capability, linking if necessary
            if !signer.getCapability<&AuthAccount>(ChildAccount.AuthAccountCapabilityPath).check() {
                self.authAccountCap = signer.linkAccount(ChildAccount.AuthAccountCapabilityPath)!
            } else {
                self.authAccountCap = signer.getCapability<&AuthAccount>(ChildAccount.AuthAccountCapabilityPath)
            }
            // Publish for the specified Address
            signer.inbox.publish(self.authAccountCap!, name: "AuthAccountCapability", recipient: parentAddress)
        }
    }
    `

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(parentAddress, t.Address)
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        if (tx.status === 4 && tx.statusCode === 0) {
          return
        }
      }
      throw "send transaction failed"
    } catch (e) {
      throw createError.InternalServerError(`account linking failed ${e}`)
    }
  }
}

module.exports = flowService