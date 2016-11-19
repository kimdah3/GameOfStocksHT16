import { NgModule }      from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { routing } from "./app.routing";

import { AppComponent } from './app.component';
import { WelcomeComponent } from "./welcome/welcome.component";

@NgModule({
    imports: [
        BrowserModule,
        routing
    ],
    declarations: [AppComponent, WelcomeComponent],
    bootstrap: [AppComponent]
})

export class AppModule { }