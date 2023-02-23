import { 
  getAccountAddress,
  deployContractByName,
  mintFlow
} from "@onflow/flow-js-testing"

export const getAdmin = async () => getAccountAddress("Admin")

export const deployCoreContracts = async (deployer) => {
  const Deployer = deployer
  await mintFlow(Deployer, 1000.0)
  await deployByName(Deployer, "core/NonFungibleToken")
  await deployByName(Deployer, "core/MetadataViews")
  await deployByName(Deployer, "core/FungibleToken")
}

export const deployBasicBeastsContracts = async (deployer) => {
  const Deployer = deployer
  await deployByName(Deployer, "basicbeasts/BasicBeasts")
}

export const deployByName = async (deployer, contractName, args) => {
  const [, error] = await deployContractByName({ to: deployer, name: contractName, args: args })
  expect(error).toBeNull()
}