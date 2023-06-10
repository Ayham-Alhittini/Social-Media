import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormControl, FormGroup, NgForm, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AccountService } from '../_services/account.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  @Output() cancelRegister = new EventEmitter<void>();
  registerForm: FormGroup;
  maxDate = new Date();

  constructor(private accountService : AccountService, private toastr : ToastrService, private fb: FormBuilder,
    private router : Router) { }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  initializeForm() {
    this.registerForm = this.fb.group({
      gender : ['male'],
      username : ['', Validators.required],
      knownAs : ['', Validators.required],
      dateOfBirth : ['', Validators.required],
      city : ['', Validators.required],
      country : ['', Validators.required],
      password : ['', 
      [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword : ['', [Validators.required, this.matchValues()]]
    });
    this.registerForm?.controls['password'].valueChanges.subscribe({
      next : () => {
        this.registerForm?.controls['confirmPassword'].updateValueAndValidity();
      }
    });
  }

  matchValues() : ValidatorFn {
    return (control: AbstractControl) => {
      return control.value === this.registerForm?.controls['password'].value ? null : {notMatching : true};
    }
  }

  register() {
    const dob = this.getDateOnly(this.registerForm.controls['dateOfBirth'].value);

    const values = {...this.registerForm.value, dateOfBirth: dob};
    
    this.accountService.register(values).subscribe({
      next : () => {
        this.router.navigateByUrl("/members");
      },
      error : error => {
        this.toastr.error("Error Occure");
        console.log(error);
      }
    });
  }
  onCancel() {
    this.cancelRegister.emit();
  }
  private getDateOnly(dob: string) : string {
    if (!dob) return '';

    let theDob = new Date(dob);

    return new Date(theDob.setMinutes(theDob.getMinutes() - theDob.getTimezoneOffset()))
      .toISOString().slice(0, 10);
  }
}
