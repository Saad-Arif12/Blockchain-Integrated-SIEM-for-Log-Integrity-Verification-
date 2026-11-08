const hre = require("hardhat");

async function main() {
  const LogIntegrity = await hre.ethers.getContractFactory("LogIntegrity");
  const logIntegrity = await LogIntegrity.deploy();
  await logIntegrity.waitForDeployment();
  const address = await logIntegrity.getAddress();
  console.log(`LogIntegrity contract deployed to: ${address}`);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});