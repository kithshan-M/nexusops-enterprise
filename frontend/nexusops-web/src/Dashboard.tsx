
import {
  LayoutDashboard,
  FolderKanban,
  CheckSquare,
  Users,
  Bell,
  Settings,
  Search,
  Plus,
  ArrowUpRight,
  ArrowDownRight,
  MoreHorizontal,
  Clock3,
  CheckCircle2,
  CircleDashed,
  Activity,
  Zap,
  Menu,
  ChevronDown,
  CalendarDays,
  Command
} from "lucide-react";

import "./App.css";

const projects = [
  {
    name: "Enterprise Banking Platform",
    category: "Backend Development",
    progress: 78,
    color: "#8b5cf6",
    tasks: "24/30",
    status: "On Track"
  },
  {
    name: "Customer Portal Redesign",
    category: "Frontend Development",
    progress: 56,
    color: "#06b6d4",
    tasks: "14/25",
    status: "In Progress"
  },
  {
    name: "Microservices Migration",
    category: "Infrastructure",
    progress: 91,
    color: "#10b981",
    tasks: "20/22",
    status: "On Track"
  }
];

const tasks = [
  {
    title: "Implement authentication service",
    project: "Enterprise Banking Platform",
    priority: "High",
    status: "In Progress",
    date: "Today"
  },
  {
    title: "Design API gateway architecture",
    project: "Microservices Migration",
    priority: "Medium",
    status: "Review",
    date: "Today"
  },
  {
    title: "Create user management module",
    project: "Customer Portal Redesign",
    priority: "High",
    status: "Pending",
    date: "Tomorrow"
  },
  {
    title: "Configure PostgreSQL database",
    project: "Microservices Migration",
    priority: "Low",
    status: "Completed",
    date: "Yesterday"
  }
];

function Dashboard({ user, onLogout }: { user: { fullName: string; email: string; roles: string[] }; onLogout: () => void }) {
  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-icon">
            <Command size={23} />
          </div>
          <span>Nexus<span className="brand-accent">Ops</span></span>
        </div>

        <div className="workspace">
          <div className="workspace-avatar">N</div>
          <div className="workspace-info">
            <strong>Nexus Technologies</strong>
            <span>Enterprise Workspace</span>
          </div>
          <ChevronDown size={16} />
        </div>

        <div className="nav-label">WORKSPACE</div>

        <nav className="navigation">
          <a className="nav-item active" href="#overview">
            <LayoutDashboard size={19} /> Overview
          </a>
          <a className="nav-item" href="#projects">
            <FolderKanban size={19} /> Projects
            <span className="nav-count">8</span>
          </a>
          <a className="nav-item" href="#tasks">
            <CheckSquare size={19} /> My Tasks
            <span className="nav-count">12</span>
          </a>
          <a className="nav-item" href="#team">
            <Users size={19} /> Team Members
          </a>
        </nav>

        <div className="nav-label">MANAGEMENT</div>

        <nav className="navigation">
          <a className="nav-item" href="#activity">
            <Activity size={19} /> Activity
          </a>
          <a className="nav-item" href="#settings">
            <Settings size={19} /> Settings
          </a>
        </nav>

        <div className="sidebar-bottom">
          <div className="upgrade-card">
            <div className="upgrade-icon"><Zap size={18} /></div>
            <strong>Boost your workflow</strong>
            <p>Explore advanced workspace productivity tools.</p>
            <button>Explore features <ArrowUpRight size={14} /></button>
          </div>

          <div className="profile">
            <div className="profile-avatar">KM</div>
            <div className="profile-info">
              <strong>{user.fullName}</strong>
              <span>{user.roles.join(", ") || "Employee"}</span>
            </div>
            <MoreHorizontal size={19} />
          </div>
        </div>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <div className="breadcrumb">
            <Menu className="mobile-menu" size={20} />
            Workspace <span>/</span> <strong>Overview</strong>
          </div>

          <div className="topbar-actions">
            <button className="search-button">
              <Search size={17} />
              <span>Search anything...</span>
              <kbd>Ctrl K</kbd>
            </button>
            <button className="icon-button notification-button">
              <Bell size={19} />
              <span className="notification-dot" />
            </button>
            <button className="top-avatar logout-avatar" title="Sign out" onClick={onLogout}>{user.fullName.split(" ").map(part => part[0]).join("").slice(0, 2).toUpperCase()}</button>
          </div>
        </header>

        <div className="page-container">
          <section className="welcome-section" id="overview">
            <div>
              <div className="eyebrow">
                <span className="live-dot" /> WORKSPACE OVERVIEW
              </div>
              <h1>Welcome, {user.fullName.split(" ")[0]} <span>👋</span></h1>
              <p>Here's what's happening across your workspace today.</p>
            </div>
            <button className="primary-button">
              <Plus size={18} /> Create project
            </button>
          </section>

          <section className="stats-grid">
            <div className="stat-card">
              <div className="stat-top">
                <span>Total Projects</span>
                <div className="stat-icon purple"><FolderKanban size={19} /></div>
              </div>
              <h2>24</h2>
              <div className="stat-footer positive">
                <ArrowUpRight size={15} /> 12.5% <span>vs last month</span>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-top">
                <span>Active Tasks</span>
                <div className="stat-icon cyan"><CheckSquare size={19} /></div>
              </div>
              <h2>86</h2>
              <div className="stat-footer positive">
                <ArrowUpRight size={15} /> 8.2% <span>vs last month</span>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-top">
                <span>Team Members</span>
                <div className="stat-icon green"><Users size={19} /></div>
              </div>
              <h2>18</h2>
              <div className="stat-footer positive">
                <ArrowUpRight size={15} /> 4.3% <span>vs last month</span>
              </div>
            </div>

            <div className="stat-card">
              <div className="stat-top">
                <span>Completed Tasks</span>
                <div className="stat-icon orange"><CheckCircle2 size={19} /></div>
              </div>
              <h2>142</h2>
              <div className="stat-footer negative">
                <ArrowDownRight size={15} /> 2.4% <span>vs last month</span>
              </div>
            </div>
          </section>

          <section className="content-grid">
            <div className="panel projects-panel" id="projects">
              <div className="panel-header">
                <div>
                  <h3>Active Projects</h3>
                  <p>Track your team's ongoing work</p>
                </div>
                <button className="text-button">View all <ArrowUpRight size={15} /></button>
              </div>

              <div className="project-list">
                {projects.map((project) => (
                  <div className="project-row" key={project.name}>
                    <div className="project-heading">
                      <div>
                        <strong>{project.name}</strong>
                        <span>{project.category}</span>
                      </div>
                      <button className="icon-button"><MoreHorizontal size={19} /></button>
                    </div>

                    <div className="progress-info">
                      <span>Progress</span>
                      <strong>{project.progress}%</strong>
                    </div>

                    <div className="progress-track">
                      <div
                        className="progress-fill"
                        style={{
                          width: `${project.progress}%`,
                          background: project.color
                        }}
                      />
                    </div>

                    <div className="project-footer">
                      <span><CheckSquare size={14} /> {project.tasks} tasks</span>
                      <span className="status-dot">
                        <span style={{ background: project.color }} />
                        {project.status}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            <div className="panel activity-panel" id="activity">
              <div className="panel-header">
                <div>
                  <h3>Workspace Activity</h3>
                  <p>Recent team updates</p>
                </div>
                <button className="icon-button"><MoreHorizontal size={20} /></button>
              </div>

              <div className="activity-list">
                <div className="activity-item">
                  <div className="activity-avatar purple">JD</div>
                  <div>
                    <p><strong>John Doe</strong> completed a task</p>
                    <span>API documentation updated</span>
                    <small>10 minutes ago</small>
                  </div>
                </div>

                <div className="activity-item">
                  <div className="activity-avatar cyan">AS</div>
                  <div>
                    <p><strong>Alex Smith</strong> created a project</p>
                    <span>Customer Portal Redesign</span>
                    <small>35 minutes ago</small>
                  </div>
                </div>

                <div className="activity-item">
                  <div className="activity-avatar orange">MK</div>
                  <div>
                    <p><strong>Michael King</strong> updated a task</p>
                    <span>Database migration</span>
                    <small>1 hour ago</small>
                  </div>
                </div>

                <div className="activity-item">
                  <div className="activity-avatar green">SJ</div>
                  <div>
                    <p><strong>Sarah Johnson</strong> joined a team</p>
                    <span>Engineering team</span>
                    <small>2 hours ago</small>
                  </div>
                </div>
              </div>
            </div>
          </section>

          <section className="panel tasks-panel" id="tasks">
            <div className="panel-header">
              <div>
                <h3>My Tasks</h3>
                <p>Stay on top of your priorities</p>
              </div>
              <button className="text-button">View all <ArrowUpRight size={15} /></button>
            </div>

            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>TASK NAME</th>
                    <th>PROJECT</th>
                    <th>PRIORITY</th>
                    <th>STATUS</th>
                    <th>DUE DATE</th>
                  </tr>
                </thead>
                <tbody>
                  {tasks.map((task) => (
                    <tr key={task.title}>
                      <td>
                        <span className="task-name">
                          {task.status === "Completed"
                            ? <CheckCircle2 size={17} />
                            : <CircleDashed size={17} />}
                          {task.title}
                        </span>
                      </td>
                      <td>{task.project}</td>
                      <td><span className={`priority ${task.priority.toLowerCase()}`}>{task.priority}</span></td>
                      <td><span className={`task-status ${task.status.toLowerCase().replace(" ", "-")}`}>{task.status}</span></td>
                      <td><span className="due-date"><Clock3 size={14} /> {task.date}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </section>

          <footer className="footer">
            <span>© 2026 NexusOps. Enterprise Workspace Platform.</span>
            <span><span className="live-dot" /> All systems operational</span>
          </footer>
        </div>
      </main>
    </div>
  );
}

export default Dashboard;
