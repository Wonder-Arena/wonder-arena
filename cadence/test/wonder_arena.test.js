import path from "path"
import {
  emulator,
  init,
  getAccountAddress,
} from "@onflow/flow-js-testing";
import {
  deployCoreContracts,
  deployByName,
  getAdmin,
  deployBasicBeastsContracts,
} from "./src/common";
import { createPlayer, getDefenderGroups, getPawns, getPlayers, getRules, register, updateGroupSize, updateMaxGroupNumber, addDefenderGroup, removeDefenderGroup, getAttackerChallenges, fight, getScores, setupRewardCollection, createReward, getRewards, claimReward, getAttackRecords, setPawnTemplates, getPlayersWithScore, addLink, getLinkedChildren } from "./src/wonder_arena";
import { bb_createTemplate, bb_getBeastIDs, bb_mintBeast, bb_setupAccount } from "./src/basicbeasts";

jest.setTimeout(1000000)

const deployContracts = async () => {
  const deployer = await getAdmin()
  await deployCoreContracts(deployer)
  await deployBasicBeastsContracts(deployer)
  await deployByName(deployer, "WonderArenaWorldRules_BasicBeasts1")
  await deployByName(deployer, "WonderArenaPawn_BasicBeasts1")
  await deployByName(deployer, "WonderArenaBattleField_BasicBeasts1")
  await deployByName(deployer, "WonderArenaReward_BasicBeasts1")
  await deployByName(deployer, "WonderArenaLinkedAccounts_BasicBeasts1")
}

describe("Deployment", () => {
  beforeEach(async () => {
    const basePath = path.resolve(__dirname, "..")
    await init(basePath)
    await emulator.start()
    return await new Promise(r => setTimeout(r, 2000));
  })

  afterEach(async () => {
    await emulator.stop();
    return await new Promise(r => setTimeout(r, 2000));
  })

  it("Deployment - Should deploy all contracts successfully", async () => {
    await deployContracts()
  })
})

describe("World Rules", () => {
  beforeEach(async () => {
    const basePath = path.resolve(__dirname, "..")
    await init(basePath)
    await emulator.start()
    await new Promise(r => setTimeout(r, 2000));
    return await deployContracts()
  })

  afterEach(async () => {
    await emulator.stop();
    return await new Promise(r => setTimeout(r, 2000));
  })

  it("Update parameters", async () => {
    const admin = await getAdmin()
    const rules = await getRules()
    expect(rules.groupSize).toBe('3')
    expect(rules.maxGroupNumber).toBe('4')

    const [res, err] = await updateGroupSize(admin, '12')
    expect(err).toBeNull()

    const [res2, err2] = await updateMaxGroupNumber(admin, '33')
    expect(err2).toBeNull()

    const rules2 = await getRules()
    expect(rules2.groupSize).toBe('12')
    expect(rules2.maxGroupNumber).toBe('33')
  })
})

describe("Pawn", () => {
  beforeEach(async () => {
    const basePath = path.resolve(__dirname, "..")
    await init(basePath)
    await emulator.start()
    await new Promise(r => setTimeout(r, 2000));
    await deployContracts()
    const admin = await getAdmin()
    const [result, error] = await setPawnTemplates(admin)
    expect(error).toBeNull()
    return
  })

  afterEach(async () => {
    await emulator.stop();
    return await new Promise(r => setTimeout(r, 2000));
  })

  it("Get Pawns", async () => {
    await setupAdmin()
    await setupAlice()
    const alice = await getAccountAddress("Alice")
    const beastIDs = await bb_getBeastIDs(alice)
    const [pawns, err] = await getPawns(alice, beastIDs)
    expect(err).toBeNull()

    expect(pawns.length).toBe(3)
    const pawn1 = pawns[0]
    expect(pawn1.nft.id).toBe(beastIDs[0])
    expect(pawn1.mana).toBe('0')
  })
})

describe("BattleField", () => {
  beforeEach(async () => {
    const basePath = path.resolve(__dirname, "..")
    await init(basePath)
    await emulator.start()
    await new Promise(r => setTimeout(r, 2000));
    await deployContracts()
    const admin = await getAdmin()
    return await setPawnTemplates(admin)
  })

  afterEach(async () => {
    await emulator.stop();
    return await new Promise(r => setTimeout(r, 2000));
  })

  it("Register", async () => {
    const alice = await getAccountAddress("Alice")
    const admin = await getAdmin()
    await setupAdmin()
    await setupAlice()

    let [, error1] = await createPlayer(alice, "AliceTeam")
    expect(error1).toBeNull()

    let [, error2] = await register(admin, alice)
    expect(error2).toBeNull()

    let players = await getPlayers()
    expect((players[0]).address).toBe(alice)
  })

  it("DefenderGroup", async () => {
    const admin = await getAdmin()
    const alice = await getAccountAddress("Alice")
    await setupAdmin()
    await setupAlice()
    await createPlayer(alice, "AliceTeam")
    await register(admin, alice)

    const beastIDs = await bb_getBeastIDs(alice)

    const group1 = [beastIDs[0], beastIDs[1], beastIDs[2]]
    const [, error1] = await addDefenderGroup(alice, "alice", "AliceGroup1", group1)
    expect(error1).toBeNull()
    const group2 = [beastIDs[2], beastIDs[1], beastIDs[0]]
    const [, error2] = await addDefenderGroup(alice, "alice", "AliceGroup2", group2)
    expect(error2).toBeNull()

    const groups = await getDefenderGroups(alice)
    expect(groups.length).toBe(2)

    const [, error3] = await addDefenderGroup(alice, "alice", "AliceGroup3", [])
    expect(error3).not.toBeNull()
    const [, error4] = await addDefenderGroup(alice, "alice", "AliceGroup4", [beastIDs[0], beastIDs[1], beastIDs[2], beastIDs[1]])
    expect(error4).not.toBeNull()

    const [, error5] = await removeDefenderGroup(alice, "AliceGroup1")
    expect(error5).toBeNull()

    const groups2 = await getDefenderGroups(alice)
    expect(groups2.length).toBe(1)
  })

  it("Fight", async () => {
    const admin = await getAdmin()
    const alice = await getAccountAddress("Alice")
    const bob = await getAccountAddress("Bob")
    await setupAdmin()

    await setupAlice()
    await createPlayer(alice, "AliceTeam")
    await register(admin, alice)

    await setupBob()
    await createPlayer(bob, "BobTeam")
    await register(admin, bob)

    const bobBeastIDs = await bb_getBeastIDs(bob)
    const group1 = [bobBeastIDs[0], bobBeastIDs[1], bobBeastIDs[2]]
    const [, error1] = await addDefenderGroup(bob, "bob", "BobGroup1", group1) 
    expect(error1).toBeNull()
    const group2 = [bobBeastIDs[3], bobBeastIDs[4], bobBeastIDs[5]]
    const [, error2] = await addDefenderGroup(bob, "bob", "BobGroup2", group2) 
    expect(error2).toBeNull()

    const aliceBeastIDs = await bb_getBeastIDs(alice)
    const attackers = [aliceBeastIDs[0], aliceBeastIDs[1], aliceBeastIDs[2]]
    const [, error3] = await addDefenderGroup(alice, "alice", "AliceGroup1", attackers) 
    expect(error3).toBeNull()

    let [, error] = await fight(admin, alice, attackers, bob)
    expect(error).toBeNull()

    let records = await getAttackRecords(alice, bob)
    console.log(records)
    let record = Object.values(records)[0]
    expect(record.attackerBeasts).not.toEqual(record.defenderBeasts)

    let players = await getPlayersWithScore()
    let negativeValues = Object.values(players).map((v) => parseInt(v.score)).filter((v) => v < 0)
    expect(negativeValues.length).toBe(0)

    await adminCreateReward()
    const [rewards, ] = await getRewards(admin)
    const rewardID = Object.keys(rewards)[0]
    let winner = alice
    let loser = bob
    let aliceScore = parseInt(players[alice].score)
    if (aliceScore < 30) {
      winner = bob
      loser = alice
    }

    const [, claimError] = await claimReward(winner, admin, rewardID)
    expect(claimError).toBeNull()

    const [, claimError2] = await claimReward(loser, admin, rewardID)
    expect(claimError2).not.toBeNull()

    const [rewards2, ] = await getRewards(admin)
    const reward = rewards2[rewardID]
    expect(reward.claimedCount).toBe('1')
  })
})

describe("Reward", () => {
  beforeEach(async () => {
    const basePath = path.resolve(__dirname, "..")
    await init(basePath)
    await emulator.start()
    await new Promise(r => setTimeout(r, 2000));
    await deployContracts()
    const admin = await getAdmin()
    return await setPawnTemplates(admin)
  })

  afterEach(async () => {
    await emulator.stop();
    return await new Promise(r => setTimeout(r, 2000));
  })

  it("Create Reward", async () => {
    const admin = await getAdmin()
    await setupAdmin()
    await setupAlice()

    const beastIDs = await bb_getBeastIDs(admin)
    const [, error] = await createReward(admin, "Reward", "", beastIDs, "30", true)
    expect(error).toBeNull()

    const [rewards, error2] = await getRewards(admin)
    expect(error2).toBeNull()
  })
})

describe("Link", () => {
  beforeEach(async () => {
    const basePath = path.resolve(__dirname, "..")
    await init(basePath)
    await emulator.start()
    return await new Promise(r => setTimeout(r, 2000));
  })

  afterEach(async () => {
    await emulator.stop();
    return await new Promise(r => setTimeout(r, 2000));
  })

  it("Add link", async () => {
    await deployContracts()
    const admin = await getAdmin()
    const alice = await getAccountAddress("Alice")
    const [res, err] = await addLink(admin, admin, alice)
    expect(err).toBeNull()
    const [res1, err1] = await getLinkedChildren(admin)
    expect(err1).toBeNull()
    expect(res1.length).toBe(1)
    expect(res1[0]).toBe(alice)
  })
})


const adminCreateReward = async () => {
  const admin = await getAdmin()
  const beastIDs = await bb_getBeastIDs(admin)
  await createReward(admin, "Reward", "", beastIDs, "30", true) 
}

const setupAdmin = async () => {
  const admin = await getAdmin()
  await bb_setupAccount(admin)

  await bb_createTemplate(admin, "Moon", "1", "Electric")
  await bb_createTemplate(admin, "Saber", "2", "Water")
  await bb_createTemplate(admin, "Shen", "3", "Grass")
  await bb_createTemplate(admin, "Azazel", "4", "Fire")

  await setupRewardCollection(admin)
  await bb_mintBeast(admin, "1", admin)
  await bb_mintBeast(admin, "2", admin)
  await bb_mintBeast(admin, "3", admin)
}

const setupAlice = async () => {
  const admin = await getAdmin()
  const alice = await getAccountAddress("Alice")
  await bb_setupAccount(alice)

  await bb_mintBeast(admin, "1", alice)
  await bb_mintBeast(admin, "2", alice)
  await bb_mintBeast(admin, "3", alice)
}

const setupBob = async () => {
  const admin = await getAdmin()
  const bob = await getAccountAddress("Bob")
  await bb_setupAccount(bob)

  await bb_mintBeast(admin, "2", bob)
  await bb_mintBeast(admin, "3", bob)
  await bb_mintBeast(admin, "4", bob)
  await bb_mintBeast(admin, "1", bob)
  await bb_mintBeast(admin, "2", bob)
  await bb_mintBeast(admin, "4", bob)
}