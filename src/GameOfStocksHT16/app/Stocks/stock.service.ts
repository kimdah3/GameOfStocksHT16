import { Injectable } from '@angular/core';
import { Http, Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/catch';

import { IStock } from './stock';

@Injectable()
export class StockService {
    private _stockUrl = 'api/stocks';
    constructor(private _http: Http) {

    }
    getStocks(): Observable<IStock[]> {
        return this._http.get(this._stockUrl)
            .map((response: Response) => <IStock[]>response.json())
            .do(data => console.log('All: ' + JSON.stringify(data)))
            .catch(this.handleError);
    }

    private handleError(error: Response) {
        console.log(error);
        return Observable.throw(error.json().error || 'Server error');
    }
}