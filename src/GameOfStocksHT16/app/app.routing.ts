import { Routes, RouterModule } from "@angular/router";

import { WelcomeComponent } from "./welcome/welcome.component";
import { StockListComponent } from "./Stocks/stock-list.component";
import { StockComponent } from "./Stocks/stock.component";
import { ProfileComponent } from "./Profile/profile.component";

const appRoutes: Routes = [
    { path: "", redirectTo: "welcome", pathMatch: "full" },
    { path: "welcome", component: WelcomeComponent },
    { path: "stocks", component: StockListComponent },
    { path: "stock/:id", component: StockComponent },
    { path: "profile", component: ProfileComponent }
];

export const routing = RouterModule.forRoot(appRoutes);