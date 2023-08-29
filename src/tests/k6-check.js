import { sleep } from 'k6';

export default function () {
    console.log('running test');
  sleep(1);
}