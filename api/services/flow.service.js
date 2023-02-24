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
    const decrypted= CryptoJS.AES.decrypt(encrypted, secret).toString(CryptoJS.enc.Utf8)
    return decrypted
  }

  static async getAdminAccount() {
    const FlowSigner = (await import('../utils/signer.mjs')).default
    const keys = (process.env.ADMIN_ENCRYPTED_PRIVATE_KEYS).split(",")
    const keyIndex = Math.floor(Math.random() * keys.length)
    const key = this.decryptPrivateKey(keys[keyIndex])

    const signer = new FlowSigner(
      process.env.ADMIN_ADDRESS, 
      key,
      keyIndex,
      {}
    )
    return signer
  }

  static async getUserSigner(flowAccount) {
    const FlowSigner = (await import('../utils/signer.mjs')).default
    const signer = new FlowSigner(
      flowAccount.address,
      this.decryptPrivateKey(flowAccount.encryptedPrivateKey),
      0,
      {}
    )

    return signer 
  }

  static generateKeypair() {
    const EC = require("elliptic").ec
    const ec = new EC("p256")
    const keypair = ec.genKeyPair()

    let privateKey = keypair.getPrivate().toString('hex')
    while (privateKey.length != 64) {
      privateKey = keypair.getPrivate().toString('hex')
    }

    const publicKey = keypair.getPublic().encode('hex').substring(2)
    return {privateKey: privateKey, publicKey: publicKey}
  }

  static isGeneratingAccounts = false

  static async generateWonderArenaAccounts() {
    if (this.isGeneratingAccounts) {
      console.log("is generating address?", this.isGeneratingAccounts)
      return
    }

    this.isGeneratingAccounts = true
    const users = await prisma.user.findMany({
      where: { flowAccount: null }
    })

    for (let i = 0; i < users.length; i++) {
      let user = users[i]
      try {
        await this.createWonderArenaAccount(user)
      } catch (e) {
        console.log(e)
      }
    }
    this.isGeneratingAccounts = false
  }

  static async createWonderArenaAccount(data) {
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

    const signer = await this.getAdminAccount()

    const {privateKey: privateKey, publicKey: publicKeyHex} = this.generateKeypair()
    const code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena
    import FungibleToken from 0x9a0766d93b6608b7
    import NonFungibleToken from 0x631e88ae7f1d7c20
    import MetadataViews from 0x631e88ae7f1d7c20
    import BasicBeasts from 0xfa252d0aa22bf86a
    import ChildAccount from 0x1b655847a90e644a
    
    transaction(name: String, publicKeyHex: String) {
        prepare(signer: AuthAccount) {
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
    .replace(WonderArenaPath, WonderArenaAddress) 

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(name, t.String),
        arg(publicKeyHex, t.String)
      ])
  
      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        let event = tx.events.find((e) => e.type == 'flow.AccountCreated')
        if (!event) {
          throw {statusCode: 500, message: "Account generation failed"}
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
      
        return flowAccount
      }
      throw "send transaction failed"
    } catch (e) {
      throw {statusCode: 500, message: `Account generation failed ${e}`}
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
    .replace(WonderArenaPath, WonderArenaAddress) 

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
    .replace(WonderArenaPath, WonderArenaAddress) 

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
      throw {statusCode: 500, message: `Add defender group failed ${e}`}
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
    .replace(WonderArenaPath, WonderArenaAddress) 

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
      throw {statusCode: 500, message: `remove defender group failed ${e}`}
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

    let signer = await this.getAdminAccount()
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
      throw {statusCode: 500, message: `Claim BasicBeasts failed ${e}`}
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
    .replace(WonderArenaPath, WonderArenaAddress) 

    const challengeTimes = await fcl.query({
      cadence: script,
      args: (arg, t) => [
        arg(user.flowAccount.address, t.Address),
        arg(defenderAddress, t.Address)
      ]
    })

    if (challengeTimes >= 3) {
      throw {statusCode: 422, message: "Can only challenge a player for 3 times at most"}
    }

    let signer = await this.getAdminAccount()
    let code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xWonderArena

    transaction(
        attackerAddress: Address,
        attackerIDs: [UInt64],
        defenderAddress: Address
    ) {
        prepare(acct: AuthAccount) {}
    
        execute {
            let attackerGroup = WonderArenaBattleField_BasicBeasts1.BeastGroup(
                name: "AttackerGroup",
                beastIDs: attackerIDs
            )
    
            WonderArenaBattleField_BasicBeasts1.fight(
                attackerAddress: attackerAddress,
                attackerGroup: attackerGroup,
                defenderAddress: defenderAddress
            )
        }
    }
    `
    .replace(WonderArenaPath, WonderArenaAddress) 

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(user.flowAccount.address, t.Address),
        arg(attackerIDs.map((id) => id.toString()), t.Array(t.UInt64)),
        arg(defenderAddress, t.Address)
      ])
  
      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        if (tx.status === 4 && tx.statusCode === 0) {
          let event = tx.events.find((e) => e.type.includes('ChallengeHappened'))
          if (!event) {
            throw "Fight failed"
          }
          return {attacker: user.flowAccount.address, defender: defenderAddress, challengeUUID: event.data.uuid}
        }
      }
      throw "send transaction failed"
    } catch (e) {
      throw {statusCode: 500, message: `Fight failed ${e}`}
    }
  }
}

module.exports = flowService