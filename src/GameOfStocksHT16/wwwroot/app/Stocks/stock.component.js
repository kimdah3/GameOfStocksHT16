"use strict";
var __decorate = (this && this.__decorate) || function (decorators, target, key, desc) {
    var c = arguments.length, r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
    if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
    else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
    return c > 3 && r && Object.defineProperty(target, key, r), r;
};
var __metadata = (this && this.__metadata) || function (k, v) {
    if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(k, v);
};
var core_1 = require("@angular/core");
var router_1 = require("@angular/router");
var stock_service_1 = require("./stock.service");
var StockComponent = (function () {
    function StockComponent(_route, _router, _stockService) {
        this._route = _route;
        this._router = _router;
        this._stockService = _stockService;
    }
    StockComponent.prototype.ngOnInit = function () {
        this.stockLabel = this._route.snapshot.params['id'];
        this.getStock(this.stockLabel);
    };
    StockComponent.prototype.getStock = function (id) {
        var _this = this;
        this._stockService.getStock(id)
            .subscribe(function (stock) { return _this.stock = stock; }, function (error) { return _this.errorMessage = error; });
    };
    StockComponent.prototype.onBack = function () {
        this._router.navigate(['/stocks']);
    };
    StockComponent = __decorate([
        core_1.Component({
            templateUrl: 'templates/stock.component.html',
            providers: [stock_service_1.StockService]
        }), 
        __metadata('design:paramtypes', [router_1.ActivatedRoute, router_1.Router, stock_service_1.StockService])
    ], StockComponent);
    return StockComponent;
}());
exports.StockComponent = StockComponent;
;
//# sourceMappingURL=stock.component.js.map