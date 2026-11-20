import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/** The root component. The page frame (header, footer) lives in core/layout/shell. */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class App {}
