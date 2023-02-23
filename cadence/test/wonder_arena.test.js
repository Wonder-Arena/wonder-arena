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
