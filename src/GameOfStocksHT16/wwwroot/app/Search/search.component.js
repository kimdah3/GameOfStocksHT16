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
var core_1 = require('@angular/core');
var stock_service_1 = require("../Stocks/stock.service");
var SearchComponent = (function () {
    function SearchComponent(_stockService) {
        this._stockService = _stockService;
    }
    SearchComponent.prototype.ngOnInit = function () {
        var _this = this;
        this._stockService.getStocks().subscribe(function (stocks) { return _this.stocks = stocks; }, function (error) { return _this.errorMessage = error; });
    };
    SearchComponent.prototype.clearSearch = function () {
        this.searchFilter = "";
    };
    SearchComponent.prototype.colorSelector = function (value) {
        if (value.charAt(0) == "+") {
            return true;
        }
    };
    SearchComponent = __decorate([
        core_1.Component({
            selector: 'navsearch',
            templateUrl: 'templates/search.component.html',
            styleUrls: ['css/search.component.css'],
            providers: [stock_service_1.StockService]
        }), 
        __metadata('design:paramtypes', [stock_service_1.StockService])
    ], SearchComponent);
    return SearchComponent;
}());
exports.SearchComponent = SearchComponent;
//# sourceMappingURL=search.component.js.map