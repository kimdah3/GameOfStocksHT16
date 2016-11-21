import { Routes, RouterModule } from "@angular/router";

import { WelcomeComponent } from "./welcome/welcome.component";
import { StockListComponent } from "./Stocks/stock-list.component";
import { StockComponent } from "./Stocks/stock.component";

const appRoutes: Routes = [
    { path: "", redirectTo: "welcome", pathMatch: "full" },
    { path: "welcome", component: WelcomeComponent },
    { path: "stocks", component: StockListComponent },
    { path: "stocks/: id", component: StockComponent }
];

export const routing = RouterModule.forRoot(appRoutes);