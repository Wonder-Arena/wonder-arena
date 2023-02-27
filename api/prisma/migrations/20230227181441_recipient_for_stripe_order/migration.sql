/*
  Warnings:

  - Added the required column `recipient` to the `StripeOrder` table without a default value. This is not possible if the table is not empty.

*/
-- AlterTable
ALTER TABLE "StripeOrder" ADD COLUMN     "recipient" TEXT NOT NULL;
