import {
  Directive,
  Input,
  TemplateRef,
  ViewContainerRef,
  OnInit,
  inject,
} from "@angular/core";
import { AuthService } from "../../core/services/auth.service";

@Directive({
  selector: "[appHasRole]",
  standalone: true,
})
export class HasRoleDirective implements OnInit {
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private authService = inject(AuthService);

  @Input() appHasRole: string[] = [];

  ngOnInit(): void {
    const userRole = this.authService.userRole;

    if (userRole && this.appHasRole.includes(userRole)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else {
      this.viewContainer.clear();
    }
  }
}
