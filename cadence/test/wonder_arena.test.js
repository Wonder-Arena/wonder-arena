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
import { getRules, updateGroupSize, updateMaxGroupNumber } from "./src/wonder_arena";


jest.setTimeout(1000000)

const deployContracts = async () => {
  const deployer = await getAdmin()
  await deployCoreContracts(deployer)
  await deployBasicBeastsContracts(deployer)
  await deployByName(deployer, "WonderArenaWorldRules_BasicBeasts1")
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
