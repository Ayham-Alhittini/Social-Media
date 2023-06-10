import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BusyService {

  constructor(private spinnerService : NgxSpinnerService) { }

  busyRequestCount = 0;
  chatRequestState = false;


  busy() {
    this.busyRequestCount++;
    this.spinnerService.show(undefined, {
      type : 'ball-climbing-dot',
      bdColor : 'rgba(255, 255, 255, 0)',
      color : '#E95420'
    });
  }

  idle() {
    this.busyRequestCount--;
    if (this.busyRequestCount <= 0) {
      this.busyRequestCount = 0;
      this.spinnerService.hide();
    }
  }

}
