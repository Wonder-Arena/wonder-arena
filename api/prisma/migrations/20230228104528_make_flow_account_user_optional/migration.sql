-- DropForeignKey
ALTER TABLE "FlowAccount" DROP CONSTRAINT "FlowAccount_userId_fkey";

-- AlterTable
ALTER TABLE "FlowAccount" ALTER COLUMN "userId" DROP NOT NULL;

-- AddForeignKey
ALTER TABLE "FlowAccount" ADD CONSTRAINT "FlowAccount_userId_fkey" FOREIGN KEY ("userId") REFERENCES "User"("id") ON DELETE SET NULL ON UPDATE CASCADE;
