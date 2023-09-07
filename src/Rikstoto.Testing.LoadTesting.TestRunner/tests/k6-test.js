import grpc from "k6/net/grpc";
import { check, sleep, group } from "k6";
import { uuidv4 } from 'https://jslib.k6.io/k6-utils/1.4.0/index.js';

const betLimitsService = new grpc.Client();
betLimitsService.load(["proto"], "BetLimitsService.proto");

const bettingService = new grpc.Client();
bettingService.load(["proto"], "BettingService.proto");

const virtualAccountService = new grpc.Client();
virtualAccountService.load(["proto"], "VirtualAccountService.proto");

export let options = {
  ext: {
    loadimpact: {
      projectID: 3631397,
      name: "PlaceBetWithAccountPayment"
    }
  }
}

export default () => {
  betLimitsService.connect(__ENV.env + "internalservices.r22test.local:9235", {
    plaintext: true,
  });
    bettingService.connect(__ENV.env + "internalservices.r22test.local:9235", {
    plaintext: true,
  });
    virtualAccountService.connect(__ENV.env + "internalservices.r22test.local:9233", {
    plaintext: true,
  });

  group("Place bet with account payment", () => {
    let purchaseId = uuidv4();
    console.log('purchaseId', purchaseId);
    let amount = {
      amount: '5600'
    };
    let customer = {
      customerId: __ENV.customerId
    };
      const betDataStrings = ['d:2023-01-16|t:MO|g:V75|nt:1|w:1|org:NR|p:50|pr:1000|o:0|s1:16384|s2:256|s3:16|s4:512|s5:32|s6:128|s7:1056'];

    AddBetLimitReservation(customer, purchaseId, amount);
    sleep(.5);
    RegisterTicketPurchaseDrafts(customer, purchaseId, betDataStrings);
    sleep(.5);
    Reserve(customer, purchaseId, amount);
    sleep(.5);
    PlaceBets(customer, purchaseId);
  });

  betLimitsService.close();
  bettingService.close();
  virtualAccountService.close();
};

function AddBetLimitReservation(customer, purchaseId, amount) {
  
  const addBetLimitReservationRequest = {
    customer: customer,
    amount: amount,
    purchaseId: purchaseId,
  };

  const addBetLimitReservationResponse = betLimitsService.invoke(
    "rikstoto.betlimitsservice.BetLimitsService/AddBetLimitReservation",
    addBetLimitReservationRequest
  );

  check(addBetLimitReservationResponse, {
    "AddBetLimitReservation: status is OK": (r) =>
      r && r.status === grpc.StatusOK,
  });

  console.log('AddBetLimitReservation', JSON.stringify(addBetLimitReservationResponse.message));
}

function RegisterTicketPurchaseDrafts(customer, purchaseId, betDataStrings) {
  const registerTicketPurchaseDraftsRequest = {
    customer: customer,
    agentKey: {
      agentId: '00801'
    },
    purchaseId: purchaseId,
    betDataStrings: betDataStrings,
    ownerTrack: {},
    originatedFrom: 1
  };

  const registerTicketPurchaseDraftsResponse = bettingService.invoke(
    "BettingService/RegisterTicketPurchaseDrafts",
    registerTicketPurchaseDraftsRequest
  );

  check(registerTicketPurchaseDraftsResponse, {
    "RegisterTicketPurchaseDrafts: status is OK": (r) =>
      r && r.status === grpc.StatusOK,
  });

  console.log('RegisterTicketPurchaseDrafts', JSON.stringify(registerTicketPurchaseDraftsResponse.message));
}

function Reserve(customer, purchaseId, amount) {
  const reserveRequest = {
    customer: customer,
    amount: amount,
    purchaseId: purchaseId,
    withoutExpiration: false,
    transactionText: '',
  };

  const reserveResponse = virtualAccountService.invoke(
    "VirtualAccountService/Reserve",
    reserveRequest
  );

  check(reserveResponse, {
    "Reserve: status is OK": (r) => r && r.status === grpc.StatusOK,
  });

  console.log('Reserve', JSON.stringify(reserveResponse.message));
}

function PlaceBets(customer, purchaseId) {
  const placeBetsRequest = {
    customer: customer,
    purchaseId: purchaseId,
  };

  const placeBetsResponse = bettingService.invoke(
    "BettingService/PlaceBets",
    placeBetsRequest
  );

  check(placeBetsResponse, {
    "PlaceBets: status is OK": (r) => r && r.status === grpc.StatusOK,
  });

  console.log('PlaceBets', JSON.stringify(placeBetsResponse.message));
}
