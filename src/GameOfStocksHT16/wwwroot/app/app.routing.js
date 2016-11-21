"use strict";
var router_1 = require("@angular/router");
var welcome_component_1 = require("./welcome/welcome.component");
var stock_list_component_1 = require("./Stocks/stock-list.component");
var appRoutes = [
    { path: "", redirectTo: "welcome", pathMatch: "full" },
    { path: "welcome", component: welcome_component_1.WelcomeComponent },
    { path: "stocks", component: stock_list_component_1.StockListComponent }
];
exports.routing = router_1.RouterModule.forRoot(appRoutes);
//# sourceMappingURL=app.routing.js.map