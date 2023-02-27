/*
  Warnings:

  - A unique constraint covering the columns `[sessionId]` on the table `StripeOrder` will be added. If there are existing duplicate values, this will fail.

*/
-- CreateIndex
CREATE UNIQUE INDEX "StripeOrder_sessionId_key" ON "StripeOrder"("sessionId");
