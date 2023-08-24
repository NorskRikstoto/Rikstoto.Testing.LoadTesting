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
  betLimitsService.connect("test3internalservices.r22test.local:9235", {
    plaintext: true,
  });
  bettingService.connect("test3internalservices.r22test.local:9235", {
    plaintext: true,
  });
  virtualAccountService.connect("test3internalservices.r22test.local:9233", {
    plaintext: true,
  });

  group("Place bet with account payment", () => {
    let purchaseId = uuidv4();
    console.log('purchaseId', purchaseId);
    let amount = {
      amount: '200'
    };
    let customer = {
      customerId: '9000427'
    };
    const betDataStrings = ['d:2022-10-22|t:BJ|g:V75|nt:1|w:1|org:NR|p:9800|f:100|pr:50|o:2|s1:2|s2:18|s3:2|s4:10|s5:6|s6:38|s7:958|l:1'];

    AddBetLimitReservation(customer, purchaseId, amount);
    sleep(1);
    RegisterTicketPurchaseDrafts(customer, purchaseId, betDataStrings);
    sleep(1);
    Reserve(customer, purchaseId, amount);
    sleep(1);
    PlaceBets(customer, purchaseId);
  });

  betLimitsService.close();
  bettingService.close;
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
    ownerTrack: {}
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
