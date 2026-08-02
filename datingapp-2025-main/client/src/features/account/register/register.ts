import { Component, inject, output, signal } from '@angular/core';
import { AbstractControl, FormArray, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { RegisterCreds } from '../../../types/user';
import { AccountService } from '../../../core/services/account-service';
import { MemberService } from '../../../core/services/member-service';
import { TextInput } from "../../../shared/text-input/text-input";
import { Router } from '@angular/router';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { Prompt } from '../../../types/member';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, TextInput, TranslatePipe],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {
  private accountService = inject(AccountService);
  private memberService = inject(MemberService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  cancelRegister = output<boolean>();
  protected creds = {} as RegisterCreds;
  protected credentialsForm: FormGroup;
  protected profileForm: FormGroup;
  protected promptsForm: FormArray;
  protected currentStep = signal(1);
  protected validationErrors = signal<string[]>([]);
  protected promptBank = signal<Prompt[]>([]);
  protected savingPrompts = signal(false);

  constructor() {
    this.credentialsForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', Validators.required],
      password: ['', [Validators.required,
      Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]]
    });

    this.profileForm = this.fb.group({
      gender: ['male', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
    })

    this.promptsForm = this.fb.array([
      this.fb.group({ promptId: [null], answer: [''] }),
      this.fb.group({ promptId: [null], answer: [''] }),
      this.fb.group({ promptId: [null], answer: [''] }),
    ]);

    this.credentialsForm.controls['password'].valueChanges.subscribe(() => {
      this.credentialsForm.controls['confirmPassword'].updateValueAndValidity();
    })
  }

  protected get promptSlots() {
    return this.promptsForm.controls as FormGroup[];
  }

  promptOptionsFor(index: number): Prompt[] {
    const chosenElsewhere = this.promptSlots
      .filter((_, i) => i !== index)
      .map(g => g.value.promptId);
    return this.promptBank().filter(p => !chosenElsewhere.includes(p.id));
  }

  matchValues(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const parent = control.parent;
      if (!parent) return null;

      const matchValue = parent.get(matchTo)?.value;
      return control.value === matchValue ? null : { passwordMismatch: true }
    }
  }

  nextStep() {
    if (this.credentialsForm.valid) {
      this.currentStep.update(prevStep => prevStep + 1);
    }
  }

  prevStep() {
    this.currentStep.update(prevStep => prevStep - 1);
  }

  getMaxDate() {
    const today = new Date();
    today.setFullYear(today.getFullYear() - 18);
    return today.toISOString().split('T')[0];
  }

  register() {
    if (this.profileForm.valid && this.credentialsForm.valid) {
      const formData = { ...this.credentialsForm.value, ...this.profileForm.value };

      this.accountService.register(formData).subscribe({
        next: () => {
          this.currentStep.set(3);
          this.memberService.getPromptBank().subscribe(bank => {
            this.promptBank.set(bank);
            this.promptSlots.forEach((group, i) => group.patchValue({ promptId: bank[i]?.id ?? null }));
          });
        },
        error: error => {
          this.validationErrors.set(error)
        }
      })
    }

  }

  finishPrompts() {
    const answers = this.promptSlots
      .map(g => g.value)
      .filter(v => v.promptId && v.answer?.trim());

    if (answers.length === 0) {
      this.router.navigateByUrl('/daily');
      return;
    }

    this.savingPrompts.set(true);
    this.memberService.savePromptAnswers(answers).subscribe({
      next: () => this.router.navigateByUrl('/daily'),
      error: () => this.router.navigateByUrl('/daily'),
      complete: () => this.savingPrompts.set(false)
    });
  }

  skipPrompts() {
    this.router.navigateByUrl('/daily');
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
