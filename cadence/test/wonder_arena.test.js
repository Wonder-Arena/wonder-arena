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
import { getPawns, getRules, updateGroupSize, updateMaxGroupNumber } from "./src/wonder_arena";
import { bb_createTemplate, bb_getBeastIDs, bb_mintBeast, bb_setupAccount } from "./src/basicbeasts";


jest.setTimeout(1000000)

const deployContracts = async () => {
  const deployer = await getAdmin()
  await deployCoreContracts(deployer)
  await deployBasicBeastsContracts(deployer)
  await deployByName(deployer, "WonderArenaWorldRules_BasicBeasts1")
  await deployByName(deployer, "WonderArenaPawn_BasicBeasts1")
  await deployByName(deployer, "WonderArenaBattleField_BasicBeasts1")
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
    expect(rules.maxGroupNumber).toBe('10')

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
    return await deployContracts()
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

const setupAdmin = async () => {
  const admin = await getAdmin()
  await bb_setupAccount(admin)

  await bb_createTemplate(admin, "Moon", "1", "Electric")
  await bb_createTemplate(admin, "Saber", "2", "Water")
  await bb_createTemplate(admin, "Shen", "3", "Grass")
  await bb_createTemplate(admin, "Azazel", "4", "Fire")
}

const setupAlice = async () => {
  const admin = await getAdmin()
  const alice = await getAccountAddress("Alice")
  await bb_setupAccount(alice)

  await bb_mintBeast(admin, "1", alice)
  await bb_mintBeast(admin, "2", alice)
  await bb_mintBeast(admin, "3", alice)
}

