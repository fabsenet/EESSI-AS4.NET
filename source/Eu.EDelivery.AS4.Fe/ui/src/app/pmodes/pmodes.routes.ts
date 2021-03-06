import { Routes } from '@angular/router';

import { CanDeactivateGuard } from './../common/candeactivate.guard';
import { MustBeAuthorizedGuard } from './../common/mustbeauthorized.guard';
import { WrapperComponent } from './../common/wrapper.component';
import { ReceivingPmodeComponent } from './receivingpmode/receivingpmode.component';
import { SendingPmodeComponent } from './sendingpmode/sendingpmode.component';

export const ROUTES: Routes = [
    {
        path: 'pmodes', component: WrapperComponent, children: [
            { path: '', pathMatch: 'full', redirectTo: 'receiving', canDeactivate: [CanDeactivateGuard] },
            { path: 'receiving', component: ReceivingPmodeComponent, data: { title: 'Receiving PMode', mode: 'receiving' }, canDeactivate: [CanDeactivateGuard], canActivate: [MustBeAuthorizedGuard] },
            { path: 'receiving/:pmode', component: ReceivingPmodeComponent, data: { title: 'Receiving PMode', mode: 'receiving', nomenu: true }, canDeactivate: [CanDeactivateGuard], canActivate: [MustBeAuthorizedGuard] },
            { path: 'sending', component: SendingPmodeComponent, data: { title: 'Sending PMode', mode: 'sending' }, canDeactivate: [CanDeactivateGuard] },
            { path: 'sending/:pmode', component: SendingPmodeComponent, data: { title: 'Sending PMode', mode: 'sending', nomenu: true }, canDeactivate: [CanDeactivateGuard] },
        ],
        data: { title: 'PModes' },
        canActivate: [MustBeAuthorizedGuard]
    }
];
